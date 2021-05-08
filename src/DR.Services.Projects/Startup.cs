using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using DR.Packages.Mongo;
using MassTransit;
using DR.Packages.MassTransit;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using DR.Frameworks.Projects.Models;
using DR.Packages.Mongo.Models;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;

namespace DR.Services.Projects
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Add health checks
            services.AddHealthChecks();
            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(2);
                options.Predicate = (check) => check.Tags.Contains("ready");
            });

            // register MongoDBContext
            services.AddMongoContext(Configuration.GetConnectionString("DefaultConnection"), cfg =>
            {
                cfg.AddMongoRepository<Event>("Events");
                cfg.AddMongoRepository<Task>("Tasks");
                cfg.AddMongoRepository<Project>("Projects");
                cfg.AddMongoRepository<ProjectUser>("ProjectUsers");
                cfg.AddMongoRepository<User>("Users");
            });

            // Register MassTransit and Consumers if RabbitMq Connection exists (read only if not)
            var rabbitMq = Configuration.GetConnectionString("RabbitMq");
            if (string.IsNullOrEmpty(rabbitMq))
            {
                services
                    .AddMassTransit(x =>
                    {
                        x.AddConsumers(Assembly.GetExecutingAssembly());
                        x.UseRabbitMq(rabbitMq, Assembly.GetExecutingAssembly());
                    })
                    .AddMassTransitHostedService();
            }

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DR Projects Service" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DR Projects Service"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("ready"),
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions());

                if (env.IsProduction())
                {
                    endpoints.MapGet("/", async context =>
                    {
                        await context.Response.WriteAsync("DR Projects Service");
                    });
                }

                endpoints.MapControllers();
            });
        }
    }
}
