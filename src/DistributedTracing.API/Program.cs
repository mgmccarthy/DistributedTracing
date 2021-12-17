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
            var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                ActivityStopped = activity =>
                {
                    //when the activity stops, take each key/value pair in baggage and assign it to the activity's tag colleciton, so the reporting tool that surface it
                    foreach (var (key, value) in activity.Baggage)
                    {
                        activity.AddTag(key, value);
                    }
                }
            };
            ActivitySource.AddActivityListener(listener);

            var host = CreateHostBuilder(args).Build();
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
                    routing.RouteToEndpoint(typeof(PlaceOrder), "DistributedTracing.Ordering.Endpoint");

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
