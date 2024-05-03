using Microsoft.AspNetCore.Mvc;
using SharedProject.Data;
using SharedProject.Models;
using System;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace SharedProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public MessagesController(ApplicationDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
        }

        [HttpPost]
        public IActionResult PostMessage(string content)
        {
            var message = new Message
            {
                Content = content,
                RandomNumber = new Random().Next(1, 10001) // Generates a random number between 1 and 10000
            };

            _context.Messages.Add(message);
            _context.SaveChanges();

            return Ok(message);
        }


        [HttpPost("process")]
        public async Task<IActionResult> Process()
        {
            var messages = await _context.Messages.Where(m => !m.Processed).ToListAsync();

            var db = _redis.GetDatabase();
            foreach (var message in messages)
            {
                message.RandomNumber *= 2;  // Example processing: doubling the number
                message.Processed = true;

                // Save processed number in Redis
                await db.StringSetAsync($"Message:{message.Id}", message.RandomNumber);
            }

            await _context.SaveChangesAsync();

            return Ok("Processed all unprocessed messages.");
        }


    }
}
