using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedTracing.FedEx.API
{
    public class Program
    {
        public const string EndpointName = "DistributedTracing.FedEx.API";

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
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
