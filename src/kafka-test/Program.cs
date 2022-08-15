using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
internal class Program
{
    private static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, collection) =>
            {
                if (args.Contains("--consumer") || args.Contains("-c"))
                {
                    collection.AddHostedService<BookConsumerService>();
                }
                else if (args.Contains("--producer") || args.Contains("-p"))
                {
                    collection.AddHostedService<BookProducerService>();
                }
                else
                {
                    throw new System.Exception("Invalid arguments. Use -p to create a producer or -c to create a consumer.");
                }
            });
    }
}
