using Microsoft.OpenApi.Models;
using SharedProject.Models;
using StackExchange.Redis;
using System;
using System.Collections;

var builder = WebApplication.CreateBuilder(args);

// Configure Redis
// //Database
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
// {
//     var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? builder.Configuration.GetConnectionString("DefaultConnection");
//     options.UseSqlServer(connectionString);
// });

//Redis
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "";
Console.WriteLine("redisConnectionString " + redisConnectionString);
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
var redisChannelProcessedMessages = new RedisChannel("processed-messages", RedisChannel.PatternMode.Literal);

//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceC API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceC API v1"));
}

// redis subscriber
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var redisContext = services.GetRequiredService<IConnectionMultiplexer>();

    redis.GetSubscriber().Subscribe(redisChannelProcessedMessages, (channel, message) =>
    {
        Console.WriteLine(message);
    });
}

// Endpoint to fetch and display all processed messages
app.MapGet("/processed-messages", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var server = redis.GetServer(redis.GetEndPoints()[0]);
    var keys = server.Keys(pattern: "Message:*");
    var messages = new List<string>();

    foreach (var key in keys)
    {
        var value = await db.StringGetAsync(key);
        messages.Add($"{value}");
    }

    return Results.Ok(messages);
});

app.Run();
