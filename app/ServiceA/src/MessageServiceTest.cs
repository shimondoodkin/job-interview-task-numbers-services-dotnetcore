

using Microsoft.AspNetCore.Mvc;
using Moq;
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
        var mockContext = new Mock<ApplicationDbContext>();
        var mockRedis = new Mock<IConnectionMultiplexer>();
        var mockSubscriber = new Mock<ISubscriber>();

        mockRedis.Setup(redis => redis.GetSubscriber(It.IsAny<CommandFlags>())).Returns(mockSubscriber.Object);

        var messageService = new MessageService();

        // Act
        var result = await messageService.sendMessage("test message", mockContext.Object, mockRedis.Object);

        // Assert
        mockContext.Verify(x => x.Messages.Add(It.IsAny<Message>()), Times.Once());
        mockContext.Verify(x => x.SaveChanges(), Times.Once());
        mockSubscriber.Verify(x => x.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<int>(), CommandFlags.None), Times.Once());
        Assert.IsType<OkObjectResult>(result);
    }
}
