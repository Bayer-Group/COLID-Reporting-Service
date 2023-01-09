using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using COLID.Graph.Metadata.Constants;
using COLID.ReportingService.Services.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace COLID.ReportingService.Services.Implementation
{
    public class ResourceStatisticsBackgroundService : BackgroundService
    {

        private readonly ILogger<ResourceStatisticsBackgroundService> _logger;
        private readonly IResourceStatisticsService _resourceStatisticsService;
        /// <summary>
        /// Class Constructor to initialize
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="resourceStatisticsService"></param>

        public ResourceStatisticsBackgroundService(ILogger<ResourceStatisticsBackgroundService> logger, IResourceStatisticsService resourceStatisticsService)
        {
            _logger = logger;
            _resourceStatisticsService = resourceStatisticsService;

        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ResourceStatisticsBackgroundService: Started ");
            //await Task.Delay(60000, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Stopwatch stpWatch = new Stopwatch();

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheTotalNumberOfResources");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheTotalNumberOfResources();
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheTotalNumberOfResources in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheTotalNumberOfResources in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheNumberOfProperties");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheNumberOfProperties();
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheNumberOfProperties in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheNumberOfProperties in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheNumberOfResourcesInRelationToNumberOfPropertyWords1");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheNumberOfResourcesInRelationToNumberOfPropertyWords(new Uri(Resource.HasLabel));
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheNumberOfResourcesInRelationToNumberOfPropertyWords1 in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheNumberOfResourcesInRelationToNumberOfPropertyWords1 in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheNumberOfResourcesInRelationToNumberOfPropertyWords2");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheNumberOfResourcesInRelationToNumberOfPropertyWords(new Uri(Resource.HasResourceDefintion));
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheNumberOfResourcesInRelationToNumberOfPropertyWords2 in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheNumberOfResourcesInRelationToNumberOfPropertyWords2 in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheNumberOfVersionsOfResources");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheNumberOfVersionsOfResources();
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheNumberOfVersionsOfResources in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheNumberOfVersionsOfResources in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheNumberOfPropertyUsageByGroupOfResource");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheNumberOfPropertyUsageByGroupOfResource(new Uri(Resource.Groups.LinkTypes));
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheNumberOfPropertyUsageByGroupOfResource in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheNumberOfPropertyUsageByGroupOfResource in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheResourceTypeCharacteristics");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheResourceTypeCharacteristics();
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheResourceTypeCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheResourceTypeCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheConsumerGroupCharacteristics");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheConsumerGroupCharacteristics();
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheConsumerGroupCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheConsumerGroupCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheLifecycleStatusCharacteristics");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheLifecycleStatusCharacteristics();
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheLifecycleStatusCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheLifecycleStatusCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    stpWatch.Reset();
                    await Task.Delay(120000, stoppingToken);

                    try
                    {
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Started CacheInformationClassificationCharacteristics");
                        stpWatch.Start();
                        await _resourceStatisticsService.CacheInformationClassificationCharacteristics();
                        stpWatch.Stop();
                        _logger.LogInformation("ResourceStatisticsBackgroundService: Finished CacheInformationClassificationCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString());
                    }
                    catch (System.Exception ex)
                    {
                        stpWatch.Stop();
                        _logger.LogError("ResourceStatisticsBackgroundService: Error CacheInformationClassificationCharacteristics in " + stpWatch.ElapsedMilliseconds.ToString() + " - " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                    }

                    await Task.Delay(14400000, stoppingToken);
                    _logger.LogInformation("ResourceStatisticsBackgroundService: Completed ");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError("ResourceStatisticsBackgroundService: Exception " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));

                }
            }
        }
    }
}
