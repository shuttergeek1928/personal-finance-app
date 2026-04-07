var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.MapReverseProxy();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
