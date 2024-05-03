
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedProject.Data;
using SharedProject.Models;
using System;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);

});

// Configure Redis
var redisConnectionString = builder.Configuration.GetConnectionString("redisDefaultConnection");

var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);



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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceB API v1"));
}

app.MapControllers();

app.MapGet("/", (ApplicationDbContext context) =>
{
    return Results.Ok("ok");
});


var process = async (ApplicationDbContext context, IConnectionMultiplexer redis) =>
{
    var messages = await context.Messages.Where(m => !m.Processed).ToListAsync();

    var db = redis.GetDatabase();
    foreach (var message in messages)
    {
        message.RandomNumber *= 2;  // Example processing: doubling the number
        message.Processed = true;

        // Save processed number in Redis
        await db.StringSetAsync($"Message:{message.Id}", message.RandomNumber);
    }

    await context.SaveChangesAsync();
};

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
