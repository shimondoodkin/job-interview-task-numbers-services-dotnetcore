
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedProject.Data;
using SharedProject.Models;
using System;
using StackExchange.Redis;
using SharedProject.Utils;
using System.Diagnostics;
using System.Text.Json;

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
var redisChannelMessages = new RedisChannel("messages", RedisChannel.PatternMode.Literal);
var redisChannelProcessedMessages = new RedisChannel("processed-messages", RedisChannel.PatternMode.Literal);

var process = async (ApplicationDbContext context, IConnectionMultiplexer redis2) =>
{
    try
    {
        // Console.WriteLine("processing");

        var messages = await context.Messages.Where(m => !m.Processed).ToListAsync(); // error here line 36

        var db = redis.GetDatabase();
        foreach (var message in messages)
        {
            message.RandomNumber *= 2;  // Example processing: doubling the number
            message.Processed = true;

            // Save processed number in Redis
            await db.StringSetAsync($"Message:{message.Id}", message.RandomNumber, TimeSpan.FromHours(24));

            await redis.GetSubscriber().PublishAsync(redisChannelProcessedMessages, message.RandomNumber);
        }

        await context.SaveChangesAsync();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceB API", Version = "v1" });
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();  // This applies pending migrations
}

// redis subscriber
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var redisContext = services.GetRequiredService<IConnectionMultiplexer>();

    var runner = new ExclusiveTaskRunner(async () =>
    {
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            var redisContext = services.GetRequiredService<IConnectionMultiplexer>();

            await process(context, redis);
        }
    });

    // process messages when starting
    await process(context, redis);

    redis.GetSubscriber().Subscribe(redisChannelMessages, (channel, message) =>
    {
        // Console.WriteLine("got message " + message.ToString());
        Task.Run(async () =>
        {
            await runner.RunExclusiveAsync(); // not blcoking the messages loop, ignore the await
        });
    });
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceB API v1"));
}


app.MapGet("/", (ApplicationDbContext context) =>
{
    return Results.Ok("ok");
});

app.MapGet("/process", async (ApplicationDbContext context, IConnectionMultiplexer redis) =>
{
    try
    {
        await process(context, redis);
        return Results.Ok("processed ok");
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithDescription("Triggers message processing manually");



app.Run();
