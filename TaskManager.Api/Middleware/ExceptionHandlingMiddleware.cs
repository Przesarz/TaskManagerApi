using Microsoft.AspNetCore.Mvc;

namespace TaskManager.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Wystapil nieobsluzony wyjatek");
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Status = context.Response.StatusCode,
                    Title = "wystapil nieoczekiwany blad",
                };

                await context.Response.WriteAsJsonAsync(problem);
            }
        }
    }
}
