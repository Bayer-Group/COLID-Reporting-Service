using System;
using COLID.Graph;
using COLID.ReportingService.Repositories.Implementation;
using COLID.ReportingService.Repositories.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace COLID.ReportingService.Repositories
{
    public static class RepositoryModules
    {
        /// <summary>
        /// This method will register all the supported functions by Repository modules.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> object for registration</param>
        public static IServiceCollection AddRepositoryModules(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            //services.AddGraphModule(configuration);
            services.AddSingletonGraphModule(configuration);
            services.AddTransient<IResourceStatisticsRepository, ResourceStatisticsRepository>();
            services.AddTransient<IContactRepository, ContactRepository>();

            return services;
        }
    }
}
