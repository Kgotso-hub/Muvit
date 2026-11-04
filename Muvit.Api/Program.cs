using Microsoft.EntityFrameworkCore;
using Muvit.Infrastructure.Data;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Infrastructure.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Infrastructure.PaymentGateways;
using Muvit.Infrastructure.DistanceCalculation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<MuvitDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
builder.Services.AddScoped<IBookingRepository, SqlBookingRepository>();
builder.Services.AddScoped<IPaymentRepository, SqlPaymentRepository>();
builder.Services.AddScoped<IPayoutRepository, SqlPayoutRepository>();

builder.Services.AddHttpClient<IPaymentGateway, PayFastPaymentGateway>();
builder.Services.AddScoped<IDistanceCalculator, HaversineDistanceCalculator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();