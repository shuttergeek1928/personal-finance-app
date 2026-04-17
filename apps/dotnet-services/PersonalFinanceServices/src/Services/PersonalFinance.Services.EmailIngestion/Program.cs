using System.Reflection;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using PersonalFinance.Services.EmailIngestion.Application.Commands;
using PersonalFinance.Services.EmailIngestion.Application.Consumers;
using PersonalFinance.Services.EmailIngestion.Application.Mappings;
using PersonalFinance.Services.EmailIngestion.Application.ParsingRules;
using PersonalFinance.Services.EmailIngestion.Application.Services;
using PersonalFinance.Services.EmailIngestion.Infrastructure.BackgroundServices;
using PersonalFinance.Services.EmailIngestion.Infrastructure.Data;
using PersonalFinance.Shared.Common.Security;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerJwtSecurity("Email Ingestion", "v1", "/gateway-email-ingestion");

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
    options.ListenAnyIP(5500); // Bind to http 5500
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<EmailIngestionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(EmailIngestionMappingProfile));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SyncGmailTransactionsCommand).Assembly));

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Register parsing rules (Strategy Pattern)
builder.Services.AddSingleton<IEmailParsingRule, BankDebitParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, SwiggyParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, ZomatoParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, EmiParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, LoanParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, CreditCardParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, SalaryParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, SubscriptionParsingRule>();
builder.Services.AddSingleton<IEmailParsingRule, ShoppingParsingRule>();

// Register services
builder.Services.AddScoped<IGmailApiService, GmailApiService>();
builder.Services.AddScoped<IEmailParserService, EmailParserService>();

// Register background service for adaptive polling
builder.Services.AddHostedService<GmailSyncBackgroundService>();

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
    x.AddConsumer<UserGmailTokensUpdatedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
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
    c.SwaggerEndpoint("./v1/swagger.json", "Email Ingestion v1");
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
            var dbContext = scope.ServiceProvider.GetRequiredService<EmailIngestionDbContext>();
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

app.MapControllers();
app.Run();
