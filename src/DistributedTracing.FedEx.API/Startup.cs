using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DistributedTracing.FedEx.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DistributedTracing.FedEx.API", Version = "v1" });
            });

            services.AddOpenTelemetryTracing(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Program.EndpointName))
                .AddAspNetCoreInstrumentation(opt => opt.Enrich = (activity, key, value) =>
                {
                    Console.WriteLine($"Got an activity named {key}");
                })
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DistributedTracing.FedEx.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
