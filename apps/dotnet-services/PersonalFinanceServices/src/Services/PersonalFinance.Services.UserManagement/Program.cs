using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Services.UserManagement.Application.Commands;
using PersonalFinance.Services.UserManagement.Application.Mappings;
using PersonalFinance.Services.UserManagement.Application.Services;
using PersonalFinance.Services.UserManagement.Infrastructure.Data;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5082, listenOption => listenOption.UseHttps()); // Bind to https port 5082
    options.ListenAnyIP(5012); // Bind to port 5012
});

builder.Services.AddDbContext<UserManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(UserMappingProfile));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));

// Add Pssword Hasher
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Add Logging
builder.Services.AddLogging();

// Add CORS
builder.Services.AddCors(options => options.AddPolicy("AllowMyOrigins", builder =>
{
    builder.WithOrigins("https://localhost:5082", "http://localhost:5012")
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseHttpsRedirection();
app.UseCors("AllowMyOrigins");
app.UseAuthorization();

app.MapControllers();

app.Run();
