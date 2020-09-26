using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransitSaga.MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace MassTransitSaga
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "api", Version = "v1" });
                c.CustomSchemaIds(i => i.FullName);
            });

            var connectionString = "Server=.;Database=masstransit_vergunningen;Trusted_Connection=True;MultipleActiveResultSets=True;";

            services.AddDbContext<VergunningStateDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddMassTransit(x =>
            {
                x.AddSagaStateMachine<VergunningStateMachine, VergunningState>()
                    //.InMemoryRepository();
                    .EntityFrameworkRepository(r =>
                    {
                        r.ConcurrencyMode = ConcurrencyMode.Pessimistic; // or use Optimistic, which requires RowVersion

                        r.AddDbContext<DbContext, VergunningStateDbContext>((provider, builder) =>
                        {
                            builder.UseSqlServer(connectionString);
                        });
                    });

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.TransportConcurrencyLimit = 100;

                    cfg.ConfigureEndpoints(context);
                });
            });

            services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                using (var serviceScope = app.ApplicationServices
                            .GetRequiredService<IServiceScopeFactory>()
                            .CreateScope())
                {
                    using (var context = serviceScope.ServiceProvider.GetService<VergunningStateDbContext>())
                    {
                        context.Database.Migrate();
                    }
                }
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "api");
                c.RoutePrefix = string.Empty;

            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
