using PersonalFinance.Shared.Common.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerJwtSecurity("Api Gateway", "v1");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Authorization policies for YARP
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

app.UseCors("AllowAll");


// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Gateway v1");
    c.SwaggerEndpoint("/gateway-users/swagger/v1/swagger.json", "User Management v1");
    c.SwaggerEndpoint("/gateway-accounts/swagger/v1/swagger.json", "Account v1");
    c.SwaggerEndpoint("/gateway-transactions/swagger/v1/swagger.json", "Transactions v1");
    c.SwaggerEndpoint("/gateway-obligations/swagger/v1/swagger.json", "Obligations v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();

app.Run();
