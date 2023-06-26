using System;
using System.Net.Http;
using COLID.Common.Logger;
using COLID.Exception;
using COLID.Identity;
using COLID.ReportingService.Repositories;
using COLID.ReportingService.Services;
using COLID.Swagger;
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;

namespace COLID.ReportingService.WebApi
{
    public partial class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public IHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration);

            Configuration = configBuilder.Build();
            Environment = env;

            Console.WriteLine("- DocumentationUrl = " + Configuration.GetConnectionString("DocumentationUrl"));
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultCorrelationId();
            services.AddCorrelationIdLogger();
            services.AddCors();

            services.AddHealthChecks();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddHttpClient("NoProxy").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    UseProxy = false,
                    Proxy = null
                };
            });

            services
                .AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            services.AddColidSwaggerGeneration(Configuration);

            services.AddIdentityModule(Configuration);
            services.AddServiceModules(Configuration);
            services.AddRepositoryModules(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseCorrelationId();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(
                options => options.SetIsOriginAllowed(x => _ = true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );

            app.UseExceptionMiddleware();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });

            app.UseColidSwaggerUI(Configuration);
        }
    }
}
