
using FluentValidation;
using Payage.Api.Common.Middleware;
using Payage.Application.Abstractions;
using Payage.Application.Features.Payments;
using Payage.Application.Features.Payments.Authorize;
using Payage.Application.Features.Payments.Authorize.Models;
using Payage.Application.Features.Payments.Capture;
using Payage.Application.Features.Payments.Capture.Models;
using Payage.Application.Features.Payments.List;
using Payage.Application.Features.Payments.Refund;
using Payage.Application.Features.Payments.Refund.Models;
using Payage.Application.Features.Payments.Shared;
using Payage.Application.Features.Payments.Void;
using Payage.Infrastructure.Db;

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

            builder.Services.AddScoped<AuthorizePaymentHandler>();
            builder.Services.AddScoped<IAuthorizePaymentRepository, AuthorizePaymentRepository>();
            builder.Services.AddScoped<IValidator<AuthorizePaymentRequest>, AuthorizePaymentValidator>();

            builder.Services.AddScoped<ICapturePaymentRepository, CapturePaymentRepository>();
            builder.Services.AddScoped<CapturePaymentHandler>();
            builder.Services.AddScoped<IValidator<CapturePaymentRequest>, CapturePaymentValidator>();

            builder.Services.AddScoped<IVoidPaymentRepository, VoidPaymentRepository>();
            builder.Services.AddScoped<VoidPaymentHandler>();

            builder.Services.AddScoped<RefundPaymentHandler>();
            builder.Services.AddScoped<IRefundPaymentRepository, RefundPaymentRepository>();
            builder.Services.AddScoped<IValidator<RefundPaymentRequest>, RefundPaymentValidator>();

            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<PaymentHandler>();

            builder.Services.AddScoped<IListPaymentsRepository, ListPaymentsRepository>();
            builder.Services.AddScoped<ListPaymentsHandler>();

            builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
            {
                var cs = builder.Configuration.GetConnectionString("PayageDb");
                return new DbConnectionFactory(cs!);
            });

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
