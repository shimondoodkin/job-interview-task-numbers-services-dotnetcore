
namespace ServiceA;


using SharedProject.Data;
using SharedProject.Models;
using StackExchange.Redis;
using System;

class MessageService
{
    RedisChannel redisChannelMessages = new RedisChannel("messages", RedisChannel.PatternMode.Literal);
    public async Task<IResult> sendMessage(String messageContent, ApplicationDbContext context, IConnectionMultiplexer redis)
    {
        try
        {
            var message = new Message
            {
                Content = messageContent,
                RandomNumber = new Random().Next(1, 10001)  // Generates a random number between 1 and 10000
            };

            context.Messages.Add(message);
            context.SaveChanges();

            await redis.GetSubscriber().PublishAsync(redisChannelMessages, message.RandomNumber);
            // Console.WriteLine("saved to context db and published a message");
            return Results.Ok(message);
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

}
