using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// Swagger
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Porta do Gateway
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

app.UseRouting();

app.MapControllers();

// Swagger consolidado (UI)
app.UseSwaggerForOcelotUI();

await app.UseOcelot();

app.Run();
