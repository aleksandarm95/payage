using FluentValidation;
using Payage.Application.Exceptions;

namespace Payage.Api.Common.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private RequestDelegate _next;
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
            catch (ValidationException vEx)
            {
                _logger.LogWarning("Validation failed for request. TraceId: {TraceId} Method: {Method} Path: {Path} Errors: {@Errors}",
                   context.TraceIdentifier, context.Request.Method, context.Request.Path, vEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "Invalid request",
                        details = vEx.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                    }
                }); ;
            }
            catch(OrderReferenceConflictException orcEx)
            {
                _logger.LogWarning(orcEx, "Order reference conflict. TraceId: {TraceId} Method: {Method} Path: {Path}",
                    context.TraceIdentifier, context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "CONFLICT",
                        message = orcEx.Message,
                        details = new[] { new { field = "orderReference", message = "Duplicate order reference" } }
                    }
                });
            }
            catch(TransactionNotFoundException tnEx)
            {
                _logger.LogWarning(tnEx, "Transaction not found. TraceId: {TraceId} Method: {Method} Path: {Path}",
                    context.TraceIdentifier, context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "TRANSACTION_NOT_FOUND",
                        message = tnEx.Message,
                        details = new[] { new { field = "id", message = "Transaction not found" } }
                    }
                });
            }
            catch(InvalidTransactionStateException invsEx)
            {
                _logger.LogWarning(invsEx, "Invalid transaction state. TraceId: {TraceId} Method: {Method} Path: {Path}",
                   context.TraceIdentifier, context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "INVALID_TRANSACTION_STATE",
                        message = invsEx.Message
                    }
                });
            }
            catch (CaptureAmountExceedsAuthorizedException caeEx)
            {
                _logger.LogWarning(caeEx, "Capture amount exceeds authorized. TraceId: {TraceId} Method: {Method} Path: {Path}",
                    context.TraceIdentifier, context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "CAPTURE_AMOUNT_EXCEEDS_AUTHORIZED",
                        message = caeEx.Message,
                        details = new[] { new { field = "amount", message = "Must be less than AUTHORIZED amount" } }
                    }
                });
            }
            catch (RefundAmountExceedsCapturedException refEx)
            {
                _logger.LogWarning(refEx, "Refund amount exceeds captured. TraceId: {TraceId} Method: {Method} Path: {Path}",
                    context.TraceIdentifier, context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "REFUND_AMOUNT_EXCEEDS_CAPTURED",
                        message = refEx.Message,
                        details = new[] { new { field = "amount", message = "Must be less than CAPTURED amount" } }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing request. TraceId: {TraceId} Method: {Method} Path: {Path}",
                    context.TraceIdentifier, context.Request.Method, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "INTERNAL_ERROR",
                        message = "An unexpected error occurred."
                    }
                });
            }
        }
    }
}
