using RabbitMQ.Client;

namespace Thunderbird.Api.Infrastructure.Messaging;

public sealed class RabbitChannelFactory
{
  private readonly ConnectionFactory _factory;

  public RabbitChannelFactory(IConfiguration configuration)
  {
    _factory = new ConnectionFactory
    {
      HostName = configuration["Rabbit:Host"] ?? "localhost",
      UserName = configuration["Rabbit:Username"] ?? "guest",
      Password = configuration["Rabbit:Password"] ?? "guest"
    };
  }

  public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
  {
    var connection = await _factory.CreateConnectionAsync(cancellationToken);
    return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
  }
}
