
using FluentValidation;
using Payage.Api.Common.Middleware;
using Payage.Api.Features.Payments.Authorize;
using Payage.Api.Features.Payments.Authorize.Models;
using Payage.Api.Features.Payments.Capture;
using Payage.Api.Features.Payments.Capture.Models;
using Payage.Api.Features.Payments.Refund;
using Payage.Api.Features.Payments.Refund.Models;
using Payage.Api.Features.Payments.Shared;
using Payage.Api.Features.Payments.Void;
using Payage.Api.Infrastructure.Db;

namespace Payage.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

            builder.Services.AddScoped<AuthorizePaymentHandler>();
            builder.Services.AddScoped<AuthorizePaymentRepository>();
            builder.Services.AddScoped<IValidator<AuthorizePaymentRequest>, AuthorizePaymentValidator>();

            builder.Services.AddScoped<CapturePaymentRepository>();
            builder.Services.AddScoped<CapturePaymentHandler>();
            builder.Services.AddScoped<IValidator<CapturePaymentRequest>, CapturePaymentValidator>();

            builder.Services.AddScoped<VoidPaymentRepository>();
            builder.Services.AddScoped<VoidPaymentHandler>();

            builder.Services.AddScoped<RefundPaymentHandler>();
            builder.Services.AddScoped<RefundPaymentRepository>();
            builder.Services.AddScoped<IValidator<RefundPaymentRequest>, RefundPaymentValidator>();

            builder.Services.AddScoped<PaymentRepository>();

            var app = builder.Build();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
