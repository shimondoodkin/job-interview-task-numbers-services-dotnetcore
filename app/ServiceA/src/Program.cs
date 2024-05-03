
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ServiceA.Data;
using ServiceA.Models;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Service A API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service A API v1"));
}


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.MapGet("/", (ApplicationDbContext context) =>
{
    return Results.Ok("ok");
});

app.MapGet("/testdb", (ApplicationDbContext context) =>
{
    try
    {
        context.Database.OpenConnection();
        context.Database.CloseConnection();
        return Results.Ok("Database connection successful.");
    }
    catch (Exception ex)
    {
        return Results.Problem("Failed to connect to database: " + ex.Message);
    }
});

app.MapPost("/messages", (MessageDto dto, ApplicationDbContext context) =>
{
    var message = new Message
    {
        Content = dto.Content,
        RandomNumber = new Random().Next(1, 10001)  // Generates a random number between 1 and 10000
    };

    context.Messages.Add(message);
    context.SaveChanges();

    return Results.Ok(message);
})
.WithName("PostMessage");


app.MapGet("/message", (ApplicationDbContext context) =>
{
    var message = new Message
    {
        Content = "test",
        RandomNumber = new Random().Next(1, 10001)  // Generates a random number between 1 and 10000
    };

    context.Messages.Add(message);
    context.SaveChanges();

    return Results.Ok(message);
})
.WithName("GetMessage");



app.Run();

// Data Transfer Object for receiving message content
public class MessageDto
{
    public required string Content { get; set; }
}
