using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Muvit.Application.Interfaces.Repositories;
using Muvit.Application.Interfaces.Services;
using Muvit.Application.Services;
using Muvit.Infrastructure.Auth;
using Muvit.Infrastructure.CommissionStrategies;
using Muvit.Infrastructure.Data;
using Muvit.Infrastructure.DistanceCalculation;
using Muvit.Infrastructure.Notifications;
using Muvit.Infrastructure.PaymentGateways;
using Muvit.Infrastructure.Repositories;

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
builder.Services.AddScoped<INotificationService, SmtpNotificationService>();
builder.Services.AddScoped<ICommissionStrategy, FlatRateCommissionStrategy>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

builder.Services.AddScoped<AuthService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SigningKey"]!))
        };
    });
builder.Services.AddAuthorization();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();