
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedProject.Data;
using SharedProject.Models;
using System;
using StackExchange.Redis;
using SharedProject.Utils;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);

});

// Configure Redis
var redisConnectionString = "redis:6379";
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
var redisChannelMessages = new RedisChannel("messages", RedisChannel.PatternMode.Literal);



var process = async Task (ApplicationDbContext context, IConnectionMultiplexer redis) =>
{
    var messages = await context.Messages.Where(m => !m.Processed).ToListAsync();

    var db = redis.GetDatabase();
    foreach (var message in messages)
    {
        message.RandomNumber *= 2;  // Example processing: doubling the number
        message.Processed = true;

        // Save processed number in Redis
        await db.StringSetAsync($"Message:{message.Id}", message.RandomNumber, TimeSpan.FromHours(24));
    }

    await context.SaveChangesAsync();
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
        await process(context, redis);
    });

    redis.GetSubscriber().Subscribe(redisChannelMessages, (channel, message) =>
    {
        var _ = runner.RunExclusiveAsync(); // not blcoking the messages loop, ignore the await
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
