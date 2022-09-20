using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

public class KafkaConfig
{
    public string Host { get; set; }
    public string Topic { get; set; }
    public string ConsumerGroupName { get; set; }
    public int TopicPartitionsNumber { get; set; }
}
