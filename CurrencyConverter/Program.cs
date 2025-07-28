using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using CurrencyConverter.Services;
using CurrencyConverter.Middlewares;
using CurrencyConverter.Conventions;
using Polly;
using OpenTelemetry.Trace;
using System.Threading.RateLimiting;
using System.Text.Json;
using OpenTelemetry.Resources;
using CurrencyConverter.Porviders;
using CurrencyConverter.Configurations;
using CurrencyConverter.Services.implementation;

public partial class Program
{
    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var env = builder.Environment;
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.Services.Configure<ExchangeRateApiOptions>(builder.Configuration.GetSection("ExchangeRateApi"));
        builder.Services.Configure<ExcludedCurrenciesOptions>(builder.Configuration.GetSection("ExcludedCurrencies"));
        builder.Services.Configure<JwtIssuerOptions>(builder.Configuration.GetSection("JwtIssuerOptions"));
        builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection("RateLimiting"));
        builder.Services.Configure<OpenTelemetryOptions>(builder.Configuration.GetSection("OpenTelemetry"));

        // Bind & validate the settings
        var exchangeRateApiOptions = builder.Configuration
            .GetSection("ExchangeRateApi")
            .Get<ExchangeRateApiOptions>() ?? throw new InvalidOperationException("Missing ExchangeRateApi config.");

        var jwtOptions = builder.Configuration
            .GetSection("JwtIssuerOptions")
            .Get<JwtIssuerOptions>() ?? throw new InvalidOperationException("Missing JwtIssuerOptions config.");

        var rateLimitingOptions = builder.Configuration
            .GetSection("RateLimiting")
            .Get<RateLimitingOptions>() ?? throw new InvalidOperationException("Missing RateLimiting config.");

        var telemetryOptions = builder.Configuration
            .GetSection("OpenTelemetry")
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();


        

        // Logging
        builder.Host.UseSerilog((context, config) =>
        {
            config.ReadFrom.Configuration(context.Configuration);
        });

        // Services
        

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Currency Converter API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new string[] { }
                }
            });
        });

        // JWT Auth
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));


        // Register the hashed key for reuse

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =signingKey
                };
            });


        builder.Services.AddAuthorization(options => {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
        });

        // Custom Services
        builder.Services.AddMemoryCache();

       
        foreach (var provider in exchangeRateApiOptions.Providers)
        {
            builder.Services.AddHttpClient(provider.Name, client =>
            {
                client.BaseAddress = new Uri(provider.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(provider.TimeoutSeconds);
            })
            .AddTransientHttpErrorPolicy(policy =>
                policy.WaitAndRetryAsync(
                    provider.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(provider.RetryBackoffSeconds, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy =>
                policy.CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: provider.CircuitBreakerFailureCount,
                    durationOfBreak: TimeSpan.FromSeconds(provider.CircuitBreakerDurationSeconds)));
        }

        if (telemetryOptions.Enabled)
        {
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CurrencyConverterAPI"))
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    if (telemetryOptions.Exporter == "otlp" && !string.IsNullOrWhiteSpace(telemetryOptions.OtlpEndpoint))
                    {
                        tracerProviderBuilder.AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(telemetryOptions.OtlpEndpoint);
                        });
                    }
                    else
                    {
                        tracerProviderBuilder.AddConsoleExporter();
                    }
                });
        }


        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var error = new
                {
                    status = 429,
                    message = "Rate limit exceeded. Please wait before making more requests.",
                    retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                        ? $"{retry.TotalSeconds}s"
                        : null
                };

                await context.HttpContext.Response.WriteAsync(JsonSerializer.Serialize(error), token);
            };
            options.AddPolicy("PerUserPolicy", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User.Identity?.Name ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingOptions.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingOptions.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitingOptions.QueueLimit,
                    }));
        });

        builder.Services.AddSingleton<SecurityKey>(signingKey);
        builder.Services.AddSingleton<IExchangeRateProviderFactory, ExchangeRateProviderFactory>();
        builder.Services.AddScoped<IExchangeRateService, ExchangeRateService>();
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddTransient<IAuthService, SimpleAuthService>();
        builder.Services.AddTransient<FrankfurterProvider>();
        
        builder.Services.AddControllers(options =>
        {
            options.Conventions.Add(new GlobalRoutePrefixConvention("api"));
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();

        app.UseMiddleware<LoggingMiddleware>();

        app.MapControllers().RequireRateLimiting("PerUserPolicy");
        return app;
    }

    public static void Main(string[] args)
    {
        var app = CreateApp(args);
        app.Run();
    }
}

