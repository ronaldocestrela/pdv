namespace Pdv.API.Middleware;

/// <summary>Assigns a correlation id per request for tracing (header echo + <see cref="HttpContext.Items"/>).</summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");

        context.Items[ItemKey] = id;

        context.Response.OnStarting(static state =>
        {
            var ctx = (HttpContext)state!;
            var correlationId = (string)ctx.Items[ItemKey]!;
            if (!ctx.Response.Headers.ContainsKey(HeaderName))
                ctx.Response.Headers.Append(HeaderName, correlationId);
            return Task.CompletedTask;
        }, context);

        await next(context);
    }
}
