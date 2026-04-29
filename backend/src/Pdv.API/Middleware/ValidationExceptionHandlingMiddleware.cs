using FluentValidation;

namespace Pdv.API.Middleware;

/// <summary>Maps FluentValidation <see cref="ValidationException"/> to HTTP 400.</summary>
public sealed class ValidationExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            if (context.Response.HasStarted)
                throw;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .Select(e => new
                {
                    property = e.PropertyName,
                    error = e.ErrorMessage,
                    code = e.ErrorCode,
                })
                .ToArray();

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Validation failed",
                errors,
            });
        }
    }
}
