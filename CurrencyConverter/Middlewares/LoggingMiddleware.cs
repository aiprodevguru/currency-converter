namespace CurrencyConverter.Middlewares
{
    using Microsoft.AspNetCore.Http;
    using Serilog;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Text.Json;
    using CurrencyConverter.Helpers;

    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        private async Task handleExceptions(HttpContext context, Exception ex, int statusCode) {
            Log.Error(JsonSerializer.Serialize(
                   new
                   {
                       context.Request.Method,
                       context.Request.Path,
                       ex.Message,
                   }
               ));
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(ResponseHelper.NoData(false, ex.Message));
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var clientId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            try
            {
                await _next(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                await handleExceptions(context, ex, StatusCodes.Status401Unauthorized);
            }
            catch (ArgumentException ex)
            {
                await handleExceptions(context, ex, StatusCodes.Status422UnprocessableEntity);
            }
            catch (NotSupportedException ex)
            {
                await handleExceptions(context, ex, StatusCodes.Status422UnprocessableEntity);
            }
            catch (Exception ex)
            {
                await handleExceptions(context, ex, StatusCodes.Status500InternalServerError);
            }
            

            stopwatch.Stop();

            Log.Information(JsonSerializer.Serialize(
                new {
                    ClientIP = clientIp,
                    ClientId = clientId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    Duration = stopwatch.ElapsedMilliseconds
                }
            ));
        }
    }
}
