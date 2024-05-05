

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using ServiceA;
using SharedProject.Data;
using SharedProject.Models;
using StackExchange.Redis;
using System;
using Xunit;


public class MessageServiceTests
{
    [Fact]
    public async Task SendMessage_SendsMessage_ReturnsOk()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().Options;
        var mockContext = new Mock<ApplicationDbContext>(options);
        var mockSet = new Mock<DbSet<Message>>();
        mockContext.Setup(m => m.Messages).Returns(mockSet.Object);

        var mockRedis = new Mock<IConnectionMultiplexer>();
        var mockSubscriber = new Mock<ISubscriber>();
        mockRedis.Setup(r => r.GetSubscriber(It.IsAny<CommandFlags>())).Returns(mockSubscriber.Object);

        var messageService = new MessageService();

        // Act
        var result = await messageService.sendMessage("test message", mockContext.Object, mockRedis.Object);

        // Assert
        mockSet.Verify(m => m.Add(It.IsAny<Message>()), Times.Once());
        mockContext.Verify(m => m.SaveChanges(), Times.Once());
        Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result);
    }
}
