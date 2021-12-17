using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Configuration.AdvancedExtensibility;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DistributedTracing.Billing.Endpoint
{
    public class Program
    {
        public const string EndpointName = "DistributedTracing.Billing.Endpoint";

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

            var host = CreateHostBuilder(args).Build();
            InitializeSqlServer(host);
            host.Run();
        }

        private static void InitializeSqlServer(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<OrderContext>();
            DbInitializer.Initialize(context);
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
                        .AddNServiceBusInstrumentation()
                        .AddSqlClientInstrumentation(opt => opt.SetDbStatementForText = true)
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

                    services.AddDbContext<OrderContext>(options => options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DistributedTracing;Trusted_Connection=True;MultipleActiveResultSets=true"));
                });
    }
}
