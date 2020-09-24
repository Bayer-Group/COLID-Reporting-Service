using COLID.Cache;
using COLID.ReportingService.WebApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace COLID.ReportingService.WebApi
{
    public partial class Startup
    {
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureServices(services);
            services.AddDistributedCacheModule(Configuration, JsonSerializerSettings.GetSerializerSettings());
        }

        public void ConfigureDevelopment(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Configure(app, env);
        }
    }
}
