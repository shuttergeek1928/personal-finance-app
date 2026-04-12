using System.Reflection;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.UserManagement.Application.Commands;
using PersonalFinance.Services.UserManagement.Application.Mappings;
using PersonalFinance.Services.UserManagement.Application.Services;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;
using PersonalFinance.Services.UserManagement.Infrastructure.Services;
using PersonalFinance.Shared.Common.Security;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerJwtSecurity("User Management", "v1", "/gateway-users");

// Configure Swagger to include XML comments (keep this part)
builder.Services.Configure<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions>(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5100); // Bind to http 5100
});

builder.Services.AddDbContext<UserManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(UserMappingProfile));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

// Add Password Hasher
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Add Token Service
builder.Services.AddScoped<ITokenService, TokenService>();

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

builder.Services.AddLogging();
// Add CORS
builder.Services.AddCors(options => options.AddPolicy("AllowMyOrigins", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly); // Registers all consumers in assembly
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("finance-rabbitmq", "/", h =>
        {
            h.Username("admin");
            h.Password("admin123");
        });
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

// app.UseHttpsRedirection(); // Disabled for Docker/Reverse Proxy compatibility
app.UseCors("AllowMyOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging(); // Add Serilog request logging

app.MapControllers();

// Apply database migrations automatically on startup with retry logic
for (int i = 0; i < 10; i++)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<UserManagementDbContext>();
            // Ensure the database is ready and apply migrations
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
            break; // Success! Exit the loop
        }
    }
    catch (Exception ex)
    {
        Log.Warning($"Database migration attempt {i + 1} failed. Retrying in 5 seconds... {ex.Message}");
        if (i == 9) // Last attempt
        {
            Log.Error(ex, "An error occurred while applying database migrations after multiple attempts");
            throw;
        }
        await Task.Delay(5000);
    }
}

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<UserManagementDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        await DbInitializer.SeedAsync(context, passwordHasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

app.Run();
