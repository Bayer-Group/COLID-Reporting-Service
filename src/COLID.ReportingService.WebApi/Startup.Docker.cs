using COLID.Cache;
using COLID.ReportingService.WebApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace COLID.ReportingService.WebApi
{
    public partial class Startup
    {
        public void ConfigureDockerServices(IServiceCollection services)
        {
            ConfigureServices(services);

            services.AddCacheModule(Configuration, JsonSerializerSettings.GetSerializerSettings());
        }

        public void ConfigureDocker(IApplicationBuilder app)
        {
            Configure(app);
        }
    }
}
