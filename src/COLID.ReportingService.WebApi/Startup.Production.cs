using COLID.Cache;
using COLID.ReportingService.Services.Implementation;
using COLID.ReportingService.WebApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace COLID.ReportingService.WebApi
{
    public partial class Startup
    {
        public void ConfigureProductionServices(IServiceCollection services)
        {
            ConfigureServices(services);
            services.AddHostedService<ResourceStatisticsBackgroundService>();

            services.AddDistributedCacheModule(Configuration, JsonSerializerSettings.GetSerializerSettings());
        }

        public void ConfigureProduction(IApplicationBuilder app)
        {
            Configure(app);
        }
    }
}
