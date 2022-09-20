using System.Net;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

public class KafkaBooksProducer : IKafkaBooksProducer
{
    private readonly ILogger<KafkaBooksProducer> _logger;
    private readonly KafkaConfig _kafkaConfig;

    public KafkaBooksProducer(ILogger<KafkaBooksProducer> logger, IOptions<KafkaConfig> kafkaConfig)
    {
        _logger = logger;
        _kafkaConfig = kafkaConfig.Value;
    }

    public async Task<DeliveryResult<int, string>> ProduceAsync(string data, CancellationToken cancellationToken = default)
    {
        var producerConfig = new ProducerConfig()
        {
            BootstrapServers = _kafkaConfig.Host,
            ClientId = Dns.GetHostName(),
            Acks = Acks.All
        };

        using var producer = new ProducerBuilder<int, string>(producerConfig).Build();

        try
        {
            var partition = new Partition(Math.Abs(data.GetHashCode() % _kafkaConfig.TopicPartitionsNumber));
            return await producer.ProduceAsync(new TopicPartition(_kafkaConfig.Topic, partition), new Message<int, string>
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