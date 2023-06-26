using System;
using System.Collections.Generic;
using COLID.Cache;
using COLID.Graph.TripleStore.Repositories;
using COLID.ReportingService.FunctionalTests.Setup;
using COLID.ReportingService.Repositories;
using COLID.ReportingService.Services;
using COLID.ReportingService.Services.Interfaces;
using COLID.ReportingService.WebApi;
using COLID.ReportingService.WebApi.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using UnitTests.Builder;

namespace COLID.ReportingService.FunctionalTests
{
    public class FunctionTestsFixture : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile(AppDomain.CurrentDomain.BaseDirectory + "appsettings.Testing.json");
                conf.AddUserSecrets<Startup>();
            });

            builder.ConfigureTestServices((services) =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
                services.AddRepositoryModules(configuration);
                services.AddServiceModules(configuration);

                services.AddMemoryCacheModule(configuration, JsonSerializerSettings.GetSerializerSettings());

                var testGraphsMapping = configuration.GetSection("FunctionalTests:Graphs").Get<Dictionary<string, string>>();
                var fakeRepo = new FakeTripleStoreRepository(testGraphsMapping);

                services.RemoveAll(typeof(IRemoteRegistrationService));
                services.AddSingleton<IRemoteRegistrationService>(provider =>
                {
                    var mockRegServices = new Mock<IRemoteRegistrationService>();
                    var cgs = new ConsumerGroupBuilder().GenerateSampleDataList();
                    mockRegServices.Setup(s => s.GetConsumerGroups()).ReturnsAsync(cgs);
                    return mockRegServices.Object;
                });

                services.RemoveAll(typeof(ITripleStoreRepository));
                services.AddSingleton<ITripleStoreRepository>(provider =>
                {
                    return fakeRepo;
                });
            });
        }
    }
}
