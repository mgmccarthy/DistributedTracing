using System;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
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
                        //https://jimmybogard.com/diagnostics-and-instrumentation-for-mongodb-and-nservicebus/
                        .AddNServiceBusInstrumentation()
                        .AddMongoDBInstrumentation()
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

                    //https://github.com/jbogard/MongoDB.Driver.Core.Extensions.DiagnosticSources
                    var mongoUrl = "mongodb://localhost:27017";
                    var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(mongoUrl));
                    clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
                    services.AddSingleton<IMongoClient>(provider => new MongoClient(clientSettings));

                    //https://kevsoft.net/2020/06/25/storing-guids-as-strings-in-mongodb-with-csharp.html
                    var pack = new ConventionPack { new GuidAsStringRepresentationConvention() };
                    ConventionRegistry.Register("GUIDs as strings Conventions", pack, type => type.Namespace.StartsWith("MongoOutbox"));

                    //services.AddScoped<Func<HttpClient>>(s => () => new HttpClient
                    //{
                    //    BaseAddress = new Uri("https://localhost:5001")
                    //});

                });
    }
}