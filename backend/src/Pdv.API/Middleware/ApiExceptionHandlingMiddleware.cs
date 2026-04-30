using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Pdv.API.Middleware;

/// <summary>
/// RFC 7807 <see cref="ProblemDetails"/> for failures, request timing logs, and correlation id.
/// </summary>
public sealed class ApiExceptionHandlingMiddleware
{
    public const string ValidationProblemType = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public const string UnexpectedProblemType = "https://tools.ietf.org/html/rfc7231#section-6.6.1";

    private static readonly JsonSerializerOptions ProblemJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ApiExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ApiExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var cid)
            ? cid?.ToString()
            : null;

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
            sw.Stop();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "HTTP {Method} {Path} -> {StatusCode} in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.Response.StatusCode,
                    sw.ElapsedMilliseconds,
                    correlationId ?? "(none)");
            }
        }
        catch (ValidationException ex)
        {
            sw.Stop();
            _logger.LogWarning(
                ex,
                "Validation failed for {Method} {Path} in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path.Value,
                sw.ElapsedMilliseconds,
                correlationId ?? "(none)");

            if (context.Response.HasStarted)
                throw;

            await WriteValidationProblemAsync(context, ex, correlationId);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(
                ex,
                "Unhandled exception for {Method} {Path} in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                context.Request.Method,
                context.Request.Path.Value,
                sw.ElapsedMilliseconds,
                correlationId ?? "(none)");

            if (context.Response.HasStarted)
                throw;

            await WriteUnexpectedProblemAsync(context, ex, correlationId);
        }
    }

    private async Task WriteValidationProblemAsync(HttpContext context, ValidationException ex, string? correlationId)
    {
        var errors = ex.Errors
            .Select(e => new { field = e.PropertyName, message = e.ErrorMessage, code = e.ErrorCode })
            .ToArray();

        var problem = new ProblemDetails
        {
            Type = ValidationProblemType,
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = errors.Length == 1
                ? errors[0].message
                : "One or more validation errors occurred.",
        };

        problem.Extensions["code"] = "validation";
        problem.Extensions["errors"] = errors;
        if (!string.IsNullOrEmpty(correlationId))
            problem.Extensions["correlationId"] = correlationId;

        await WriteProblemAsync(context, problem);
    }

    private async Task WriteUnexpectedProblemAsync(HttpContext context, Exception ex, string? correlationId)
    {
        var problem = new ProblemDetails
        {
            Type = UnexpectedProblemType,
            Title = "Unexpected error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = _environment.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred.",
        };

        problem.Extensions["code"] = "unexpected";
        if (!string.IsNullOrEmpty(correlationId))
            problem.Extensions["correlationId"] = correlationId;

        await WriteProblemAsync(context, problem);
    }

    private static Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.Clear();
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json; charset=utf-8";
        var json = JsonSerializer.Serialize(problem, ProblemJsonOptions);
        return context.Response.WriteAsync(json);
    }
}
