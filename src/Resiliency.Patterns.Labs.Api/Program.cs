using Resiliency.Patterns.Labs.Api.Configuration;
using Resiliency.Patterns.Labs.Api.Services;
using Resiliency.Patterns.Labs.Api.Services.Interfaces;

using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<IHttpBinService, HttpBinService>();
builder.Services.AddSingleton<ClientPolicy>();

builder.Host.UseSerilog((_, lc) => lc.WriteTo.Console(LogEventLevel.Debug));

builder.Services.AddOptions<HttpBinSettings>().BindConfiguration("HttpBinService");

builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public abstract partial class Program
{
}