using System;
using Elsa.Activities.Rebus.Extensions;
using Elsa.Samples.RebusWorker.Messages;
using Elsa.Samples.RebusWorker.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using Rebus.Config;
using Rebus.DataBus.InMem;
using Rebus.Logging;
using Rebus.Persistence.InMem;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using YesSql.Provider.Sqlite;

namespace Elsa.Samples.RebusWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddElsa(option => option
                            .UsePersistence(db => db.UseSqLite("Data Source=elsa.db;Cache=Shared"))
                            .ConfigureServiceBus(ConfigureRebus))
                        .AddConsoleActivities()
                        .AddTimerActivities(options => options.SweepInterval = Duration.FromSeconds(1))
                        .AddRebusActivities<Greeting>()
                        .AddWorkflow<ProducerWorkflow>()
                        .AddWorkflow<ConsumerWorkflow>();
                });

        private static RebusConfigurer ConfigureRebus(RebusConfigurer rebus, IServiceProvider serviceProvider)
        {
            return rebus
                .Logging(logging => logging.ColoredConsole(LogLevel.Info))
                .Subscriptions(s => s.StoreInMemory(new InMemorySubscriberStore()))
                .DataBus(s => s.StoreInMemory(new InMemDataStore()))
                .Routing(r => r.TypeBased().Map<Greeting>("greeting"))
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "inbox"));
        }
    }
}