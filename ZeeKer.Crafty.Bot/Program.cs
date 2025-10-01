using Microsoft.EntityFrameworkCore;
using ZeeKer.Crafty.Abstractions.Configuration;
using ZeeKer.Crafty.Bot.Extensions;
using ZeeKer.Crafty.Client;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelegramBotOptions>(
    builder.Configuration.GetSection("TelegramBot"));
builder.Services.AddOptions<TelegramBotOptions>()
    .ValidateDataAnnotations();

var options = builder.Configuration.GetSection("CraftyController").Get<CraftyControllerOptions>();
ArgumentNullException.ThrowIfNull(options);

builder.Services
    .AddStorage(builder.Configuration, builder.Environment.ContentRootPath)
    .AddBotServices()
    .AddCraftyClient(options);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

using var scope = app.Services.CreateScope();
scope.ServiceProvider.Migrate();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();

app.Run();
