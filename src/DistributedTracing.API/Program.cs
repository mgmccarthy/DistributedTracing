using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using System.Diagnostics;
using NServiceBus.Configuration.AdvancedExtensibility;
using DistributedTracing.Messages;

namespace DistributedTracing.API
{
    public class Program
    {
        public const string EndpointName = "DistributedTracing.API";

        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                ActivityStopped = activity =>
                {
                    foreach (var (key, value) in activity.Baggage)
                    {
                        activity.AddTag(key, value);
                    }
                }
            };
            ActivitySource.AddActivityListener(listener);
            var host = CreateHostBuilder(args).Build();
            //SeedDb(host);
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseNServiceBus(_ =>
                {
                    var endpointConfiguration = new EndpointConfiguration(EndpointName);

                    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                    transport.ConnectionString("host=localhost");
                    transport.UseConventionalRoutingTopology();

                    var routing = transport.Routing();
                    //routing.RouteToEndpoint(typeof(SaySomething).Assembly, "NsbActivities.WorkerService");
                    routing.RouteToEndpoint(typeof(PlaceOrder), "DistributedTracing.Ordering.Endpoint");

                    //endpointConfiguration.UsePersistence<LearningPersistence>();

                    endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

                    endpointConfiguration.EnableInstallers();

                    endpointConfiguration.SendOnly();

                    endpointConfiguration.AuditProcessedMessagesTo("NsbActivities.Audit");

                    var settings = endpointConfiguration.GetSettings();

                    settings.Set(new NServiceBus.Extensions.Diagnostics.InstrumentationOptions
                    {
                        CaptureMessageBody = true
                    });

                    // configure endpoint here
                    return endpointConfiguration;
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
