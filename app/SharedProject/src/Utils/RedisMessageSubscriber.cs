namespace SharedProject.Utils
{

    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using StackExchange.Redis;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class RedisMessageSubscriber : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisChannel _channel;
        private readonly Func<CancellationToken, string?, string?, Task> _onMessage;

        public RedisMessageSubscriber(
            IConnectionMultiplexer redis,
            RedisChannel redisChannel,
            Func<CancellationToken, string?, string?, Task> onMessage)
        {
            _redis = redis;
            _channel = redisChannel;
            _onMessage = onMessage;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.SubscribeAsync(_channel, async (channel, message) =>
            {
                await _onMessage(stoppingToken, channel, message);
            });

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}