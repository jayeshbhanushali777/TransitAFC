using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using TransitAFC.Gateway.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
    });

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Transit AFC API Gateway",
//        Version = "v1",
//        Description = "Unified API Gateway for Transit AFC Microservices",
//        Contact = new OpenApiContact
//        {
//            Name = "Transit AFC Team",
//            Email = "support@transitafc.com"
//        }
//    });

//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Scheme = "Bearer"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer"
//                }
//            },
//            Array.Empty<string>()
//        }
//    });
//});
builder.Services.AddSwaggerGen();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT Secret is not configured.");
}

if (jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
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

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", cancellationToken: token);
    };
});

// CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowReactAndServices", policy =>
//    {
//        policy.WithOrigins(
//            "http://localhost:3000",   // React app
//            "https://localhost:3000",   // React app HTTPS
//            "http://localhost:7001",  
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
    options.AddPolicy("AllowReactAndServices", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// HTTP Clients
builder.Services.AddHttpClient<MicroservicesHealthCheckService>();
builder.Services.AddHttpClient<SwaggerService>();

// Services
builder.Services.AddSingleton<SwaggerService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<MicroservicesHealthCheckService>("microservices");

// Health Checks UI
builder.Services.AddHealthChecksUI(opt =>
{
    opt.SetEvaluationTimeInSeconds(30);
    opt.MaximumHistoryEntriesPerEndpoint(60);
    opt.SetApiMaxActiveRequests(1);
    opt.AddHealthCheckEndpoint("Gateway", "/health");
}).AddInMemoryStorage();

// Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
        c.RoutePrefix = "swagger";

        //// Add microservice endpoints
        c.SwaggerEndpoint("https://localhost:7001/swagger/v1/swagger.json", "User Service");
        c.SwaggerEndpoint("https://localhost:7002/swagger/v1/swagger.json", "Route Service");
        c.SwaggerEndpoint("https://localhost:7003/swagger/v1/swagger.json", "Booking Service");
        c.SwaggerEndpoint("https://localhost:7004/swagger/v1/swagger.json", "Payment Service");
        c.SwaggerEndpoint("https://localhost:7005/swagger/v1/swagger.json", "Ticket Service");
    });
}

app.UseHttpsRedirection();

// Use CORS
var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "Production";
app.UseCors("AllowReactAndServices");

//// Create a single swagger endpoint that combines all services
//app.MapGet("/swagger/v1/swagger.json", async (HttpClient httpClient) =>
//{
//    try
//    {
//        // Fetch from the downstream API
//        var response = await httpClient.GetStringAsync("https://localhost:7001/swagger/v1/swagger.json");
//        var json = JObject.Parse(response);

//        // Modify the swagger document to use gateway URLs
//        json["servers"] = new JArray(
//            new JObject { ["url"] = "https://localhost:7000" }
//        );

//        // Update all paths to include /api prefix
//        if (json["paths"] != null)
//        {
//            var paths = (JObject)json["paths"];
//            var newPaths = new JObject();

//            foreach (var path in paths)
//            {
//                var newPath = path.Key.StartsWith("/api") ? path.Key : $"/api{path.Key}";
//                newPaths[newPath] = path.Value;
//            }

//            json["paths"] = newPaths;
//        }

//        return Results.Content(json.ToString(), "application/json");
//    }
//    catch (Exception ex)
//    {
//        // Return a basic swagger document if API is not available
//        var fallbackSwagger = new JObject
//        {
//            ["openapi"] = "3.0.1",
//            ["info"] = new JObject
//            {
//                ["title"] = "TransitAFC Gateway API",
//                ["version"] = "v1"
//            },
//            ["servers"] = new JArray(
//                new JObject { ["url"] = "https://localhost:7000" }
//            ),
//            ["paths"] = new JObject(),
//            ["components"] = new JObject()
//        };

//        return Results.Content(fallbackSwagger.ToString(), "application/json");
//    }
//});

// Rate Limiting
app.UseRateLimiter();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    //context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

//// Health Checks UI
//app.MapHealthChecks("/health");
//app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

app.MapControllers();

// Ocelot Middleware (must be last)
await app.UseOcelot();

Log.Information("API Gateway started successfully");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}