using CurrencyConverter.Middlewares;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Sinks.TestCorrelator;
using System.Text.Json;
using Xunit;

namespace CurrencyConverter.Test.Middlewares
{
    public class LoggingMiddlewareTests
    {
        private async Task<(HttpContext context, string responseJson)> ExecuteMiddleware(RequestDelegate next)
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/test";
            context.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;

            context.Response.Body = new MemoryStream();

            // Create middleware
            var middleware = new LoggingMiddleware(next);

            await middleware.InvokeAsync(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string json = await new StreamReader(context.Response.Body).ReadToEndAsync();
            return (context, json);
        }

        [Fact]
        public async Task InvokeAsync_ShouldCallNext_WhenNoException()
        {
            using (TestCorrelator.CreateContext())
            {
                Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

                // Arrange
                RequestDelegate next = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status200OK;
                    return Task.CompletedTask;
                };

                // Act
                var (context, json) = await ExecuteMiddleware(next);

                // Assert
                Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
                Assert.Empty(json);

                // ✅ Verify final request log
                var logs = TestCorrelator.GetLogEventsFromCurrentContext();
                Assert.NotEmpty(logs);
                Assert.Contains(logs, log => log.RenderMessage().Contains("GET"));
            }
        }

        [Theory]
        [InlineData(typeof(UnauthorizedAccessException), StatusCodes.Status401Unauthorized, "Invalid credentials")]
        [InlineData(typeof(ArgumentException), StatusCodes.Status422UnprocessableEntity, "Bad argument")]
        [InlineData(typeof(NotSupportedException), StatusCodes.Status422UnprocessableEntity, "Not supported")]
        [InlineData(typeof(System.Exception), StatusCodes.Status500InternalServerError, "Server error")]
        public async Task InvokeAsync_ShouldHandleExceptionsCorrectly(Type exceptionType, int expectedStatus, string message)
        {
            using (TestCorrelator.CreateContext())
            {
                Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

                // Arrange
                RequestDelegate next = _ => throw (Exception)Activator.CreateInstance(exceptionType, message)!;

                // Act
                var (context, json) = await ExecuteMiddleware(next);

                // Assert
                Assert.Equal(expectedStatus, context.Response.StatusCode);

                var result = JsonSerializer.Deserialize<JsonElement>(json);
                Assert.False(result.GetProperty("success").GetBoolean());
                Assert.Equal(message, result.GetProperty("message").GetString());

                // ✅ Verify both error log AND final log are captured
                var logs = TestCorrelator.GetLogEventsFromCurrentContext();
                Assert.True(logs.Count() >= 2); // One for error, one for request summary
                Assert.Contains(logs, log => log.RenderMessage().Contains(message));
            }
        }
    }
}
