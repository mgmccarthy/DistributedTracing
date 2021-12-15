using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DistributedTracing.API
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

            //services.AddDbContext<WeatherContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DistributedTracing.API", Version = "v1" });
            });

            services.AddOpenTelemetryTracing(builder => builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(Program.EndpointName))
                .AddAspNetCoreInstrumentation(opt => opt.Enrich = (activity, key, value) =>
                {
                    Console.WriteLine($"Got an activity named {key}");
                })
                //.AddSqlClientInstrumentation(opt => opt.SetDbStatementForText = true)
                .AddNServiceBusInstrumentation()
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
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DistributedTracing.API v1"));
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
