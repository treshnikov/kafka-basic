using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += delegate
        {
            cts.Cancel();
        };

        // init db
        using (var scope = host.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;
            var context = serviceProvider.GetRequiredService<BooksDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        await host.RunAsync(cts.Token);

        Console.WriteLine("Application stopped");
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
         Host.CreateDefaultBuilder(args)
         .ConfigureServices((context, services) =>
         {
             services.AddDbContext<BooksDbContext>(opt =>
             {
                 opt.UseNpgsql(AppConfig.DbConnectionString);
             });
             services.AddScoped<IBooksDbContext, BooksDbContext>();

             services.AddHostedService<BooksProducerService>();
             services.AddHostedService<BooksConsumerService>();
         });
}
