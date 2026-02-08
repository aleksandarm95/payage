using FluentValidation;
using Payage.Api.Common.Exceptions;
using Payage.Api.Features.Payments.Authorize;
using System.Net;

namespace Payage.Api.Common.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException vEx)
            {
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
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "INVALID_TRANSACTION_STATE",
                        message = invsEx.Message,
                        details = new[] { new { field = "status", message = "Expected AUTHORIZED" } }
                    }
                });
            }
            catch (CaptureAmountExceedsAuthorizedException caeEx)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "CAPTURE_AMOUNT_EXCEEDS_AUTHORIZED",
                        message = caeEx.Message,
                        details = new[] { new { field = "amount", message = "Must be less than authorized amount" } }
                    }
                });
            }
            catch (Exception ex)
            {
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
