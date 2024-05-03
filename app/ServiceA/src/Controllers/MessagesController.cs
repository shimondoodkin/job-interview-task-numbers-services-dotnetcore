using Microsoft.AspNetCore.Mvc;
using ServiceA.Data;
using ServiceA.Models;
using System;

namespace ServiceA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MessagesController(ApplicationDbContext context)
        {
            _context = context;
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
    }
}
