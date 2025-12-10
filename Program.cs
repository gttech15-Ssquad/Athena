using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using virtupay_corporate.Data;
using virtupay_corporate.Helpers;
using virtupay_corporate.Repositories;
using virtupay_corporate.Services;
using DotNetEnv;

// Load environment variables
Env.Load();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/virtupay-corporate-.txt", rollingInterval: Serilog.RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Virtupay-Corporate")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

 // Load configuration
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default-secret-key-min-32-characters-long-for-testing";
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "VirtupayCorpAPI";
  var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "VirtupayCorpClient";
    var dbConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? "Data Source=virtupay-corporate.db";

    // Add services to the container
    builder.Services.AddControllers();
    
    // Add Entity Framework Core with SQLite
    builder.Services.AddDbContext<CorporateDbContext>(options =>
        options.UseSqlite(dbConnectionString)
);

  // Add API documentation
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
  {
        options.SwaggerDoc("v1", new OpenApiInfo
 {
 Title = "Virtupay Corporate API",
         Version = "v1.0",
 Description = "Production-ready corporate banking virtual card management system",
         Contact = new OpenApiContact
{
       Name = "Virtupay Support",
     Email = "support@virtupay.com"
  },
            License = new OpenApiLicense
{
    Name = "Internal Use"
      }
      });

    // Include XML comments for API documentation in Swagger
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
      var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
       options.IncludeXmlComments(xmlPath);
 }

        // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
            In = ParameterLocation.Header,
 Description = "JWT Authorization header using the Bearer scheme. Enter your token without the 'Bearer ' prefix - it will be added automatically.",
    Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
    BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
          new string[] { }
}
        });
    });

    // Configure JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
     {
     var key = Encoding.ASCII.GetBytes(jwtSecret);
options.TokenValidationParameters = new TokenValidationParameters
         {
       ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
    ValidateAudience = true,
    ValidAudience = jwtAudience,
    ValidateLifetime = true,
  ClockSkew = TimeSpan.FromSeconds(5)
        };

            // Add event handlers to debug authentication
       options.Events = new JwtBearerEvents
            {
        OnAuthenticationFailed = context =>
       {
     var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
       logger.LogError($"Authentication failed: {context.Exception.Message}");
  return Task.CompletedTask;
                },
             OnTokenValidated = context =>
     {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
   var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        logger.LogInformation($"Token validated for user: {userId}");
return Task.CompletedTask;
        },
                OnChallenge = context =>
         {
     var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogWarning($"JWT Challenge: {context.ErrorDescription}");
     return Task.CompletedTask;
      }
   };
        });

    // Configure Authorization
    builder.Services.AddAuthorization();

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

    // Register repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    builder.Services.AddScoped<ICardRepository, CardRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

    // Register services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICardService, CardServiceImpl>();
    builder.Services.AddScoped<ICardLimitService, CardLimitServiceImpl>();
    builder.Services.AddScoped<IBalanceService, BalanceServiceImpl>();
    builder.Services.AddScoped<ITransactionService, TransactionServiceImpl>();
    builder.Services.AddScoped<IApprovalService, ApprovalServiceImpl>();
    builder.Services.AddScoped<IAuditService, AuditService>();
 builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();
    builder.Services.AddScoped<IAccountBalanceService, AccountBalanceServiceImpl>();

    // Register helpers
    builder.Services.AddSingleton<IJwtTokenHelper>(new JwtTokenHelper(jwtSecret, jwtIssuer, jwtAudience));
    builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();

    // Add HTTP context accessor for user information
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
      app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Virtupay Corporate API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    // Use CORS - AllowAll for development
    app.UseCors("AllowAll");

    // Use HTTPS redirection
    app.UseHttpsRedirection();

  // Use authentication and authorization - ORDER MATTERS!
    app.UseAuthentication();
    app.UseAuthorization();

    // Map controllers
    app.MapControllers();

    // Initialize database
    using (var scope = app.Services.CreateScope())
    {
      var dbContext = scope.ServiceProvider.GetRequiredService<CorporateDbContext>();

        try
        {
      // For development, use EnsureCreatedAsync to create the database schema
  if (app.Environment.IsDevelopment())
         {
   await dbContext.Database.EnsureCreatedAsync();
            }
      else
            {
                // For production, use migrations
    await dbContext.Database.MigrateAsync();
      }

            // Seed demo data
        await DbSeeder.SeedAsync(dbContext);
        }
      catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while initializing the database");
            throw;
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during startup");
}
finally
{
    await Log.CloseAndFlushAsync();
}
