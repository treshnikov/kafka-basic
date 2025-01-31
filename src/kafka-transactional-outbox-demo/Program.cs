﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Events;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console(theme: SystemConsoleTheme.Literate)
            .CreateLogger();

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
         .ConfigureAppConfiguration(opt =>
         {
             opt.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
         })
         .UseSerilog()
         .ConfigureServices((context, services) =>
         {
             services.Configure<DbConfig>(context.Configuration.GetSection("DbConfig"));
             services.Configure<KafkaConfig>(context.Configuration.GetSection("KafkaConfig"));
             
             services.AddTransient<ITransactionalOutboxHandler<Book, BookOutbox>, BooksTransactionalOutboxHandler>();
             services.AddTransient<IKafkaBooksProducer, KafkaBooksProducer>();
             services.AddMediatR(typeof(Program));
             services.AddDbContext<BooksDbContext>(opt =>
             {
                var dbConnectionString = context.Configuration["DbConfig:DbConnectionString"];
                opt.UseNpgsql(dbConnectionString);
             });
             services.AddScoped<IBooksDbContext, BooksDbContext>();

             services.AddHostedService<BooksProducerBackgroundService>();
             services.AddHostedService<BooksConsumerBackgroundService>();
         });
}
