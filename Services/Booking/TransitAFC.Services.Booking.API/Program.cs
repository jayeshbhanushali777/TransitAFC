using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TransitAFC.Services.Booking.API.Mapping;
using TransitAFC.Services.Booking.API.Services;
using TransitAFC.Services.Booking.Infrastructure;
using TransitAFC.Services.Booking.Infrastructure.Repositories;
using TransitAFC.Shared.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Transit AFC Booking Service", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database Configuration
var connectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("LocalConnection")
    : builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BookingDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(BookingMappingProfile));

// HTTP Clients
builder.Services.AddHttpClient<IRouteService, RouteService>();

// Repositories
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IBookingHistoryRepository, BookingHistoryRepository>();

// Services
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IFareCalculationService, FareCalculationService>();
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
if (!string.IsNullOrEmpty(jwtSecret))
{
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
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Background Services
builder.Services.AddHostedService<BookingCleanupService>();

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
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

if (!string.IsNullOrEmpty(jwtSecret))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapControllers();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BookingDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Ensuring booking database is created...");

        if (app.Environment.IsDevelopment())
        {
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Booking database ensured created successfully.");
        }
        else
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Booking database migrated successfully.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating/migrating the booking database.");

        if (app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

app.Run();