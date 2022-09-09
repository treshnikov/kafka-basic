using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var cts = new CancellationTokenSource();
        var services = host.Services.GetServices<IWorker>();
        foreach (var s in services)
        {
            // fire and forget with no await
            s.StartAsync(cts.Token);
        }

        Console.CancelKeyPress += delegate
        {
            cts.Cancel();
        };

        await host.RunAsync(cts.Token);
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IWorker, BookProducerService>();
                services.AddSingleton<IWorker, BookConsumerService>();
            });
    }

    static void ListGroups(string bootstrapServers)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();

        var groups = adminClient.ListGroups(TimeSpan.FromSeconds(10));

        Console.WriteLine($"Consumer Groups:");
        foreach (GroupInfo? g in groups)
        {
            Console.WriteLine($"  Group: {g.Group} {g.Error} {g.State}");
            Console.WriteLine($"  Broker: {g.Broker.BrokerId} {g.Broker.Host}:{g.Broker.Port}");
            Console.WriteLine($"  Protocol: {g.ProtocolType} {g.Protocol}");
            Console.WriteLine($"  Members:");
            foreach (var m in g.Members)
            {
                Console.WriteLine($"    {m.MemberId} {m.ClientId} {m.ClientHost}");
                Console.WriteLine($"    Metadata: {m.MemberMetadata.Length} bytes");
                Console.WriteLine($"    Assignment: {m.MemberAssignment.Length} bytes");
            }
        }
    }
}
