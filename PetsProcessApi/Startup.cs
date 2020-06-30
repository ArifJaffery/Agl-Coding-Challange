using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetsProcessApi.Extensions;

namespace PetsProcessApi
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        [Obsolete]
        Microsoft.AspNetCore.Hosting.IHostingEnvironment HostingEnvironment { get; }

        [Obsolete]
        public Startup(Microsoft.AspNetCore.Hosting.IHostingEnvironment env, IConfiguration configuration)
        {
            HostingEnvironment = env ?? throw new ArgumentNullException(nameof(env));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        protected virtual void AddMvc(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            AddMvc(services);
            services.AddApplicationInsightsTelemetry();
            services.AddCommonTelemetry();
            services.AddDefaultOutboundClientCredentialsServices("PetProcessApiIda");
            services.AddConnectors(_configuration, HostingEnvironment);
            services.AddOrchestrations();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
