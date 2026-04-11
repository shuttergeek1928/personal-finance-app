using MassTransit;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.Accounts.Application.Commands;
using PersonalFinance.Services.Accounts.Application.Mappings;
using PersonalFinance.Services.Accounts.Application.Services;
using PersonalFinance.Services.Accounts.Infrastructure.Data;
using PersonalFinance.Shared.Common.Security;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerJwtSecurity("Accounts", "v1", "/gateway-accounts");

// Configure Swagger to include XML comments
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
    options.ListenAnyIP(5200); // Bind to http 5200
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();


builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(AccountMappingProfile));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAccountCommand).Assembly));

// Add Token Service
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Accounts v1");
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
            var dbContext = scope.ServiceProvider.GetRequiredService<AccountDbContext>();
            await dbContext.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
            break;
        }
    }
    catch (Exception ex)
    {
        Log.Warning($"Database migration attempt {i + 1} failed. Retrying... {ex.Message}");
        if (i == 9) throw;
        await Task.Delay(5000);
    }
}

app.Run();
