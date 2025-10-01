using Microsoft.EntityFrameworkCore;
using ZeeKer.Crafty.Configuration;
using ZeeKer.Crafty.Client;
using ZeeKer.Crafty.Storage;
using ZeeKer.Crafty.Bot.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelegramBotOptions>(
    builder.Configuration.GetSection("TelegramBot"));
builder.Services.AddOptions<TelegramBotOptions>()
    .ValidateDataAnnotations();


builder.Services
    .AddStorage(builder.Configuration, builder.Environment.ContentRootPath)
    .AddBotServices()
    .AddCraftyClient(builder.Configuration);

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
