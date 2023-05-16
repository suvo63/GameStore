using System.Diagnostics;

namespace GameStore.Api.Middleware;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(ILogger<RequestTimingMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch= new Stopwatch();
        try
        {
            stopwatch.Start();
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var elapsedMiliseconds= stopwatch.ElapsedMilliseconds;
            _logger.LogInformation(
                "{RequestMethod} {Requestpath} request took {EllapsedMilliseconds}ms to complete",
                context.Request.Method,
                context.Request.Path,
                elapsedMiliseconds
            );
        }
    }
}