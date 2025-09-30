using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TransitAFC.Services.User.API.Mapping;
using TransitAFC.Services.User.API.Services;
using TransitAFC.Services.User.Infrastructure;
using TransitAFC.Services.User.Infrastructure.Repositories;
using TransitAFC.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "User API v1",
        Version = "v1",
        Description = "TransitAFC User Management API v1"
    });
});

// Database Configuration for Google Cloud SQL
var connectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("LocalConnection")
    : builder.Configuration.GetConnectionString("DefaultConnection");

// Database
builder.Services.AddDbContext<UserDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Local development with Docker PostgreSQL
        options.UseNpgsql(connectionString);
    }
    else
    {
        // Production with Google Cloud SQL
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null);
        });
    }
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(UserMappingProfile));

// Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured. Set it in appsettings.json or JWT_SECRET environment variable.");
}

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowGateway", policy =>
//    {
//        policy.WithOrigins(
//            "https://localhost:7000",  // API Gateway
//            "http://localhost:7000",   // API Gateway HTTP
//            "http://localhost:3000",   // React app
//            "https://localhost:3000",   // React app HTTPS
//            "https://localhost:7001",
//            "https://localhost"
//        )
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .AllowCredentials();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGateway", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});



var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine($"API Request: {context.Request.Method} {context.Request.Path}");
    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
    context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
    await next();
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API v1 Custom");
    //    c.RoutePrefix = "swagger";
    //});
}

app.UseHttpsRedirection();
app.UseCors("AllowGateway");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    if (app.Environment.IsDevelopment())
    {
        context.Database.EnsureCreated();
    }
    else
    {
        context.Database.Migrate();
    }
}

app.Run();