using Confluent.Kafka;

public interface IKafkaBooksProducer
{
    Task<DeliveryResult<int, string>> ProduceAsync(string data, CancellationToken cancellationToken = default);
}
