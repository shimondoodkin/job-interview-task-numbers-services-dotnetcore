
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SharedProject.Data;
using SharedProject.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);

});

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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service A API v1"));
}

app.MapControllers();

app.MapGet("/", (ApplicationDbContext context) =>
{
    return Results.Ok("ok");
});


app.MapGet("/message", (ApplicationDbContext context) =>
{
    try
    {
        var message = new Message
        {
            Content = "test",
            RandomNumber = new Random().Next(1, 10001)  // Generates a random number between 1 and 10000
        };

        context.Messages.Add(message);
        context.SaveChanges();

        return Results.Ok(message);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithDescription("creates a message with 'test' as message content and a random number");


app.MapPost("/message", (MessageDto dto, ApplicationDbContext context) =>
{
    try
    {
        var message = new Message
        {
            Content = dto.Content,
            RandomNumber = new Random().Next(1, 10001)  // Generates a random number between 1 and 10000
        };

        context.Messages.Add(message);
        context.SaveChanges();

        return Results.Ok(message);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500);
    }
})
.WithDescription("creates a message with given Content as message content and a random number");


app.Run();

// Data Transfer Object for receiving message content
public class MessageDto
{
    public required string Content { get; set; }
}
