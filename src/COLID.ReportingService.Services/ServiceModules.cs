using System;
using COLID.Identity;
using COLID.ReportingService.Services.Configuration;
using COLID.ReportingService.Services.Implementation;
using COLID.ReportingService.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace COLID.ReportingService.Services
{
    public static class ServiceModules
    {
        /// <summary>
        /// This will register all the supported functionality by Services module.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> object for registration</param>
        public static IServiceCollection AddServiceModules(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.Configure<ColidRegistrationServiceTokenOptions>(configuration.GetSection("ColidRegistrationServiceTokenOptions"));

            services.AddTransient<IResourceStatisticsService, ResourceStatisticsService>();
            services.AddTransient<IContactService, ContactService>();
            services.AddTransient<IRemoteRegistrationService, RemoteRegistrationService>();

            return services;
        }
    }
}
