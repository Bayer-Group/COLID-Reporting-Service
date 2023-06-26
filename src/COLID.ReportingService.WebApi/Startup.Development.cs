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
        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            ConfigureServices(services);
            services.AddDistributedCacheModule(Configuration, JsonSerializerSettings.GetSerializerSettings());
            services.AddHostedService<ResourceStatisticsBackgroundService>();

        }

        public void ConfigureDevelopment(IApplicationBuilder app)
        {
            Configure(app);
        }
    }
}
