
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedProject.Data;
using SharedProject.Models;
using StackExchange.Redis;
using System;
using ServiceA;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.

//Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

//Redis
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "redis:6379";
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton(new MessageService());


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceA API", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();  // This applies pending migrations
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceA API v1"));
}

app.MapGet("/", (ApplicationDbContext context) =>
{
    return Results.Ok("ok");
});

app.MapGet("/message", (MessageService messageService, ApplicationDbContext context, IConnectionMultiplexer redis) =>
{
    return messageService.sendMessage("test", context, redis);
})
.WithDescription("creates a message with 'test' as message content and a random number")
.WithOpenApi();

app.MapPost("/message", (MessageDto dto, MessageService messageService, ApplicationDbContext context, IConnectionMultiplexer redis) =>
{
    return messageService.sendMessage(dto.Content, context, redis);
})
.WithDescription("creates a message with given Content as message content and a random number")
.WithOpenApi();

app.Run();

// Data Transfer Object for receiving message content
public class MessageDto
{
    public required string Content { get; set; }
}
