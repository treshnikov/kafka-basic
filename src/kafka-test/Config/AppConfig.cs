using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

public static class AppConfig
{
    public const string Host = "localhost:9092";
    public const string DbConnectionString = "Host=localhost;Port=2462;Database=kafka-postgresql;Username=postgres;Password=postgres";
    public const string Topic = "books-topic";
    public const string ConsumerGroupName = "book-consumers";
}
