using System.Text;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ExpenseTrackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Configure JWT authentication with enhanced debugging
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Configuration["Jwt:Key"];
        
        // Debug output to verify JWT key is being loaded
        Console.WriteLine($"JWT Key found: {!string.IsNullOrEmpty(jwtKey)}");
        if (!string.IsNullOrEmpty(jwtKey))
        {
            Console.WriteLine($"JWT Key length: {jwtKey.Length} characters");
        }
        
        if (string.IsNullOrEmpty(jwtKey))
        {
            // Provide more helpful error message with configuration paths
            var configPath = builder.Environment.IsDevelopment() 
                ? "appsettings.Development.json" 
                : "appsettings.json";
                
            Console.WriteLine($"WARNING: JWT key is missing from configuration. Check your {configPath} file.");
            Console.WriteLine("For local development, ensure you have a Jwt:Key section in your appsettings.Development.json");
            
            // For development only - use a fallback key to prevent crashes during local testing
            if (builder.Environment.IsDevelopment())
            {
                jwtKey = "your-development-fallback-key-at-least-16-chars-long";
                Console.WriteLine("Using fallback development key for JWT validation");
            }
            else
            {
                throw new InvalidOperationException("JWT key is missing from configuration.");
            }
        }

        // Add detailed JWT validation event logging
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT Authentication failed: " + context.Exception.Message);
                if (context.Exception is SecurityTokenSignatureKeyNotFoundException)
                {
                    Console.WriteLine("Signature key not found error - check that the same key is used for token creation and validation");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT Token validated successfully");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                Console.WriteLine($"JWT Token received: {(string.IsNullOrEmpty(token) ? "No token" : "Token present")}");
                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,  // Simplified for local development
            ValidateAudience = false, // Simplified for local development
            ClockSkew = TimeSpan.Zero
        };
    });

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ExpenseTracker API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add API Explorer (needed for Swagger)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExpenseTracker API v1"));
    
    // Add middleware to print out current configuration in development
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/debug-jwt-config")
        {
            var config = app.Configuration;
            await context.Response.WriteAsync($"JWT Key exists: {!string.IsNullOrEmpty(config["Jwt:Key"])}\n");
            await context.Response.WriteAsync($"Environment: {app.Environment.EnvironmentName}\n");
            await context.Response.WriteAsync("Check console for more detailed JWT debugging information");
        }
        else
        {
            await next();
        }
    });
}

//app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Print startup message
Console.WriteLine($"Application starting in {app.Environment.EnvironmentName} mode");
Console.WriteLine($"JWT authentication is configured. Validating issuer: {false}, audience: {false}");

app.Run();
