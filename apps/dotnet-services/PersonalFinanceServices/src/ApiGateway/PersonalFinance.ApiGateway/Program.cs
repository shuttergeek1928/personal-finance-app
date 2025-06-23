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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api Gateway v1");
    c.SwaggerEndpoint("/gateway-users/swagger/v1/swagger.json", "User Management v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.MapReverseProxy();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
