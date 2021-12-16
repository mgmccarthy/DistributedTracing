using System;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DistributedTracing.Ordering.Endpoint
{
    public class Program
    {
        public const string EndpointName = "DistributedTracing.Ordering.Endpoint";

        public static void Main(string[] args)
        {
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

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseNServiceBus(hostBuilderContext =>
                {
                    var endpointConfiguration = new EndpointConfiguration(EndpointName);

                    endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

                    var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                    transport.ConnectionString("host=localhost");
                    transport.UseConventionalRoutingTopology();

                    var routing = transport.Routing();
                    //routing.RouteToEndpoint(typeof(MakeItYell).Assembly, "NsbActivities.ChildWorkerService");

                    //endpointConfiguration.UsePersistence<LearningPersistence>();

                    endpointConfiguration.EnableInstallers();

                    endpointConfiguration.AuditProcessedMessagesTo("DistributedTracing.Audit");

                    var settings = endpointConfiguration.GetSettings();

                    settings.Set(new NServiceBus.Extensions.Diagnostics.InstrumentationOptions
                    {
                        CaptureMessageBody = true
                    });

                    return endpointConfiguration;
                })
                .ConfigureServices((_, services) =>
                {
                    services.AddOpenTelemetryTracing(builder => builder
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(EndpointName))
                        .AddNServiceBusInstrumentation()
                        //.AddHttpClientInstrumentation()
                        .AddZipkinExporter(o =>
                        {
                            o.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                        })
                        .AddJaegerExporter(c =>
                        {
                            c.AgentHost = "localhost";
                            c.AgentPort = 6831;
                        })
                    );

                    //services.AddScoped<Func<HttpClient>>(s => () => new HttpClient
                    //{
                    //    BaseAddress = new Uri("https://localhost:5001")
                    //});
                });
    }
}