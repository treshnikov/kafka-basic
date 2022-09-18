using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;

public class KafkaBooksProducer : IKafkaBooksProducer
{
    private readonly ILogger<KafkaBooksProducer> _logger;

    public KafkaBooksProducer(ILogger<KafkaBooksProducer> logger)
    {
        _logger = logger;
    }

    public async Task<DeliveryResult<int, string>> ProduceAsync(string data, CancellationToken cancellationToken = default)
    {
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = AppConfig.Host,
            ClientId = Dns.GetHostName(),
            Acks = Acks.All
        };

        using var producer = new ProducerBuilder<int, string>(producerConfig).Build();

        try
        {
            var partition = new Partition(Math.Abs(data.GetHashCode() % AppConfig.TopicPartitionsNumber));
            return await producer.ProduceAsync(new TopicPartition(AppConfig.Topic, partition), new Message<int, string>
            {
                Key = partition.Value,
                Value = data
            }, cancellationToken);
        }
        catch (ProduceException<int, string> ex)
        {
            _logger.LogWarning($"A publisher exception has occured: {ex.Message}");
            throw;
        }
    }
}