
using FluentValidation;
using Payage.Api.Common.Middleware;
using Payage.Api.Features.Payments.Authorize;
using Payage.Api.Features.Payments.Authorize.Models;
using Payage.Api.Features.Payments.Capture;
using Payage.Api.Features.Payments.Capture.Model;
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
