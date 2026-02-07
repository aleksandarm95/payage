using FluentValidation;
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
