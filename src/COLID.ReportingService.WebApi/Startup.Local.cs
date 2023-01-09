using COLID.Cache;
using COLID.ReportingService.WebApi.Settings;
using COLID.ReportingService.Services.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace COLID.ReportingService.WebApi
{
    public partial class Startup
    {
        public void ConfigureLocalServices(IServiceCollection services)
        {
            ConfigureServices(services);

            services.AddCacheModule(Configuration, JsonSerializerSettings.GetSerializerSettings());
            services.AddHostedService<ResourceStatisticsBackgroundService>();

        }

        public void ConfigureLocal(IApplicationBuilder app)
        {
            Configure(app);
        }
    }
}
