using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using COLID.Cache.Services;
using COLID.Common.Utilities;
using COLID.Graph.Metadata.Constants;
using COLID.Graph.Metadata.Services;
using COLID.ReportingService.Common.Constants;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.Repositories.Interfaces;
using COLID.ReportingService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace COLID.ReportingService.Services.Implementation
{
    public class ResourceStatisticsService : IResourceStatisticsService
    {
        private readonly IResourceStatisticsRepository _resourceStatisticsRepository;
        private readonly IMetadataService _metadataService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<ResourceStatisticsService> _logger;
        private readonly int defaultExpirationTime_4hours = 14400;

        public ResourceStatisticsService(IResourceStatisticsRepository resourceStatisticsRepository, IMetadataService metadataService, ICacheService cacheService, ILogger<ResourceStatisticsService> logger)
        {
            _resourceStatisticsRepository = resourceStatisticsRepository;
            _metadataService = metadataService;
            _cacheService = cacheService;
            _logger = logger;
        }

        /// <summary> 
        /// Caches the total number of resources from the resource and metadata graph to redis cache. 
        /// </summary> 
        public async Task CacheTotalNumberOfResources()
        {
            try
            {
                _cacheService.Delete(CacheKeys.Statistics.TotalNumberOfResources, () => _resourceStatisticsRepository.GetTotalNumberOfResources(null));
                var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
                var allResources = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalNumberOfResources, () => _resourceStatisticsRepository.GetTotalNumberOfResources(types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
                _logger.LogInformation($"ResourceStatisticsService: Repository Value GetTotalNumberOfResources.... Key : {CacheKeys.Statistics.TotalNumberOfResources} Value : {allResources}");
                _logger.LogInformation("ResourceStatisticsService: Set CacheTotalNumberOfResources.... ");
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation($"ResourceStatisticsService: CacheTotalNumberOfResources Exception... {ex.Message}");

            }
        }
        /// <summary> 
        /// Returns the amount of all resources in the database. Draft and publish are evaluated as one resource only. 
        /// </summary> 
        /// <returns>Amount of properties</returns> 
        public string GetTotalNumberOfResources()
        {
            if (_cacheService.Exists(CacheKeys.Statistics.TotalNumberOfProperties, () => _resourceStatisticsRepository.GetTotalNumberOfResources(null)) == false)
            {
                return string.Empty;
            }
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _cacheService.GetOrAdd(CacheKeys.Statistics.TotalNumberOfResources, () => _resourceStatisticsRepository.GetTotalNumberOfResources(types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
        }

        /// <summary> 
        /// Caches the total number of properties. 
        /// </summary> 
        public async Task CacheNumberOfProperties()
        {
            _cacheService.Delete(CacheKeys.Statistics.TotalNumberOfProperties, () => _resourceStatisticsRepository.GetAllPropertiesByResourceTypes(null));
            _cacheService.Delete(CacheKeys.Statistics.TotalUsageCountByPredicate, () => _resourceStatisticsRepository.GetUsageOfProperties(null));
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            // adding PID_Concept and other parent concepts as well because we need all properties of parent and grandparents too
            var parentsToAdd = new List<string> { PIDO.PidConcept, PIDO.NonRDFDataset, PIDO.RDFDataset, CacheKeys.Resource.COLIDResources, CacheKeys.Resource.CoreEntities };
            ((List<string>)types).AddRange(parentsToAdd);
            
            var results = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalNumberOfProperties, () => _resourceStatisticsRepository.GetAllPropertiesByResourceTypes(types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            
            var filteredList = results.Where(s => s.IsMandatory == false &&
            s.GroupType != Resource.Groups.LinkTypes &&
            s.GroupType != Resource.Groups.DistributionEndpoints &&
            s.GroupType != Resource.Groups.InvisibleTechnicalInformation).ToList();

            
            var cachedProperty = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalUsageCountByPredicate, () =>
                _resourceStatisticsRepository.GetUsageOfProperties(filteredList.Select(s => s.PropertyUri).Distinct().ToList()), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            
            _logger.LogInformation("ResourceStatisticsService: Set CacheNumberOfProperties.... ");
        }


        /// <summary> 
        /// Returns an object containing a list of properties and how many resources this property has from redis cache. 
        /// </summary> 
        /// <returns></returns> 
        public PropertyStatistics GetNumberOfProperties()
        {
            try
            {
                if (_cacheService.Exists(CacheKeys.Statistics.TotalNumberOfProperties, () => _resourceStatisticsRepository.GetAllPropertiesByResourceTypes(null)) == false)
                {
                    return new PropertyStatistics();
                }
                if (_cacheService.Exists(CacheKeys.Statistics.TotalUsageCountByPredicate, () => _resourceStatisticsRepository.GetUsageOfProperties(null)) == false)
                {
                    return new PropertyStatistics();
                }
                var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);

                var properties = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalNumberOfProperties, () => _resourceStatisticsRepository.GetAllPropertiesByResourceTypes(types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
                var filteredList = properties.Where(s => s.IsMandatory == false &&
                       s.GroupType != Resource.Groups.LinkTypes &&
                       s.GroupType != Resource.Groups.DistributionEndpoints &&
                       s.GroupType != Resource.Groups.InvisibleTechnicalInformation).ToList();

                var propertyUsage = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalUsageCountByPredicate, () =>
                _resourceStatisticsRepository.GetUsageOfProperties(filteredList.Select(s => s.PropertyUri).Distinct().ToList()), TimeSpan.FromSeconds(defaultExpirationTime_4hours));

                var totalResourcesByType = GetResourceTypeCharacteristics();
                var resourceTypeHierarchy = _metadataService.GetResourceTypeHierarchy(null);

                //Add Prent resource Types
                var parentsToAdd = new List<string> { PIDO.PidConcept, PIDO.NonRDFDataset, PIDO.RDFDataset, CacheKeys.Resource.COLIDResources, CacheKeys.Resource.CoreEntities };
                foreach (var parent in parentsToAdd)
                {
                    //Get list of child resourece types for the parent resource type
                    var returnlist = new List<string>();
                    GetAllChildResourceTypesofParentResourceType(resourceTypeHierarchy, parent, returnlist);

                    //Loop through to sum up all child resource type's total count 
                    int totPublishCount = 0;
                    int totDraftCount = 0;
                    foreach (var resType in returnlist)
                    {
                        var curPropertyTypeUsage = totalResourcesByType.Where(s => s.Key == resType).FirstOrDefault();
                        if (curPropertyTypeUsage != null)
                        {
                            totPublishCount += curPropertyTypeUsage.PublishedCount;
                            totDraftCount += curPropertyTypeUsage.DraftCount;
                        }                        
                    }

                    //Add these parent resourcetypes along with child resource types beacuse some of the properties are declared at parent level
                    totalResourcesByType.Add(new PropertyCharacteristic
                    {
                        Key = parent,
                        DraftCount = totDraftCount,
                        PublishedCount = totPublishCount
                    });
                }


                //Initialize the output list
                var resultForChart = properties.Where(x=>x.IsMandatory == false &&
                                        x.GroupType != Resource.Groups.LinkTypes &&
                                        x.GroupType != Resource.Groups.DistributionEndpoints &&
                                        x.GroupType != Resource.Groups.InvisibleTechnicalInformation).
                Select(prop => new PropertyStatisticItem
                {
                    Property = prop.PropertyUri,
                    Key = prop.PropertyName,
                    Value = "0",
                    Total = "0"
                }).ToList();

                //Populate Value and Total
                foreach (var prop in resultForChart)
                {
                    //Update count of Property used
                    var curProperty = propertyUsage.Where(i => i.PropertyUri == prop.Property).FirstOrDefault();
                    if (curProperty != null)
                        prop.Value = curProperty.UsageCount.ToString();

                    //Find ResourceTypes in which the property was used
                    var curResourceTypes = properties.Where(s => s.PropertyUri == prop.Property).ToList();
                    int totResources = 0;
                    foreach(var resType in curResourceTypes)
                    {
                        var curPropertyTypeUsage = totalResourcesByType.Where(s => s.Key == resType.ResourceType).FirstOrDefault();
                        if (curPropertyTypeUsage != null)
                            totResources += curPropertyTypeUsage.PublishedCount;
                    }
                    prop.Total = totResources.ToString();
                }

                return new PropertyStatistics("Amount of properties", 0, resultForChart.OrderBy(o => o.Key).ToList());
            }
            catch(System.Exception ex)
            {
                _logger.LogError("ResourceStatisticsService:" + (ex.InnerException == null ? ex.Message : ex.InnerException.Message));
                return new PropertyStatistics();
            }
        }

        /// <summary>
        /// Return list of all child resourcetypes under the provided parent resource type
        /// </summary>
        /// <param name="resourceHierarchy"></param>
        /// <param name="parentResourceType"></param>
        /// <param name="returnList"></param>
        /// <param name="found"></param>
        private void GetAllChildResourceTypesofParentResourceType(Graph.TripleStore.DataModels.Base.EntityTypeDto resourceHierarchy, string parentResourceType, List<string> returnList, bool found = false)
        {
            if (resourceHierarchy.Id == parentResourceType)
                found = true;

            if (found)
            {
                if (resourceHierarchy.SubClasses.Count == 0)
                {
                    returnList.Add(resourceHierarchy.Id);
                }
                else
                {
                    foreach (var childHierarchy in resourceHierarchy.SubClasses)
                    {
                        GetAllChildResourceTypesofParentResourceType(childHierarchy, parentResourceType, returnList, found);
                    }
                }
            }
            else
            {
                foreach (var childHierarchy in resourceHierarchy.SubClasses)
                {
                    GetAllChildResourceTypesofParentResourceType(childHierarchy, parentResourceType, returnList, found);
                }
            }
        }

        /// <summary> 
        /// Specifies the selection options for the given property key and their amount of uses. 
        /// </summary> 
        /// <param name="predicate">Property key</param> 
        /// <returns></returns> 
        public PropertyStatistics GetNumberOfControlledVocabularySelection(Uri predicate)
        {
            Guard.IsValidUri(predicate);
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var numberOfControlledVocabulary = _cacheService.GetOrAdd($"numberOfControlledVocabulary:{predicate}",
                  () => _resourceStatisticsRepository.GetNumberOfControlledVocabularySelection(predicate, types));
            return numberOfControlledVocabulary;
        }

        /// <summary> 
        /// Caches the selection options for the given property key and their amount of uses. 
        /// <param name="prprty">Property key</param> 
        /// </summary> 
        public async Task CacheNumberOfResourcesInRelationToNumberOfPropertyWords(Uri prprty)
        {
            _cacheService.Delete($"{CacheKeys.Statistics.TotalPropertyValuesOfAllResources}:{prprty}", () => _resourceStatisticsRepository.GetPropertyValuesOfAllResources(prprty, null));
            _cacheService.Delete($"{CacheKeys.Statistics.TotalNumberOfResourcesByPredicate}:{prprty}", () => _resourceStatisticsRepository.GetTotalNumberOfResourcesByPredicate(prprty));
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var cachedTotalPropertyValue = _cacheService.GetOrAdd($"{CacheKeys.Statistics.TotalPropertyValuesOfAllResources}:{prprty}", () =>
                _resourceStatisticsRepository.GetPropertyValuesOfAllResources(prprty, types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            var cachedTotalPropertyPredicates = _cacheService.GetOrAdd($"{CacheKeys.Statistics.TotalNumberOfResourcesByPredicate}:{prprty}", () =>
                _resourceStatisticsRepository.GetTotalNumberOfResourcesByPredicate(prprty), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            _logger.LogInformation("ResourceStatisticsService: Set CacheNumberOfResourcesInRelationToNumberOfPropertyWords.... ");

        }

        /// <summary> 
        /// Returns the amount of words for a specific property from redis cache. 
        /// </summary> 
        /// <param name="predicate">Property key</param> 
        /// <param name="increment">The increment indicates the steps in which the number is to be separated.</param> 
        /// <returns></returns> 
        public PropertyStatistics GetNumberOfResourcesInRelationToNumberOfPropertyWords(Uri predicate, int increment)
        {
            Guard.IsValidUri(predicate);
            Guard.IsGreaterThanZero(increment);

            if (_cacheService.Exists($"{CacheKeys.Statistics.TotalPropertyValuesOfAllResources}:{predicate}", () => _resourceStatisticsRepository.GetPropertyValuesOfAllResources(predicate, null)) == false)
            {
                return new PropertyStatistics();
            }
            if (_cacheService.Exists($"{CacheKeys.Statistics.TotalNumberOfResourcesByPredicate}:{predicate}", () => _resourceStatisticsRepository.GetTotalNumberOfResourcesByPredicate(predicate)) == false)
            {
                return new PropertyStatistics();
            }
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var results = _cacheService.GetOrAdd($"{CacheKeys.Statistics.TotalPropertyValuesOfAllResources}:{predicate}", () =>
                _resourceStatisticsRepository.GetPropertyValuesOfAllResources(predicate, types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            var countedResults = results.Item2.Select(t => WordCount(t)).ToList();
            var total = _cacheService.GetOrAdd($"{CacheKeys.Statistics.TotalNumberOfResourcesByPredicate}:{predicate}", () =>
                _resourceStatisticsRepository.GetTotalNumberOfResourcesByPredicate(predicate), TimeSpan.FromSeconds(defaultExpirationTime_4hours));

            return new PropertyStatistics(results.Item1, increment, CreateIncrementResultOfList(countedResults, increment, total));                        
        }

        /// <summary> 
        /// Caches the resources version information. 
        /// </summary> 
        public async Task CacheNumberOfVersionsOfResources()
        {
            _cacheService.Delete(CacheKeys.Statistics.TotalNumberOfVersionsOfResources, () => _resourceStatisticsRepository.GetNumberOfVersionsOfResources(null));
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var cachedTotalVersions = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalNumberOfVersionsOfResources, () =>
                _resourceStatisticsRepository.GetNumberOfVersionsOfResources(types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            _logger.LogInformation("ResourceStatisticsService: Set CacheNumberOfResourcesInRelationToNumberOfPropertyWords.... ");
        }
        /// <summary> 
        /// Indicates how many reosources has how many versions. Only one resource per version chain is used for analysis, so that the other resources are not considered further. 
        /// </summary> 
        /// <param name="increment">The increment indicates the steps in which the number is to be separated.</param> 
        /// <returns></returns> 
        public PropertyStatistics GetNumberOfVersionsOfResources(int increment)
        {
            Guard.IsGreaterThanZero(increment);
            if (_cacheService.Exists(CacheKeys.Statistics.TotalNumberOfVersionsOfResources, () => _resourceStatisticsRepository.GetNumberOfVersionsOfResources(null)) == false)
            {
                return new PropertyStatistics();
            }
            
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var results = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalNumberOfVersionsOfResources, () =>
            _resourceStatisticsRepository.GetNumberOfVersionsOfResources(types), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            var total = GetTotalNumberOfResources();
            
            return new PropertyStatistics("Amount of resource versions", increment, CreateIncrementResultOfList(results, increment, total));            
        }

        /// <summary> 
        /// Caches properties of a given group per resource. 
        /// </summary> 
        public async Task CacheNumberOfPropertyUsageByGroupOfResource(Uri group)
        {
            _cacheService.Delete($"{CacheKeys.Statistics.TotalNumberOfPropertyUsageByGroupOfResource}:{group}", () => _resourceStatisticsRepository.GetNumberOfPropertyUsageByGroupOfResource(group));
            var cachedPropertyPerGroup = _cacheService.GetOrAdd($"{CacheKeys.Statistics.TotalNumberOfPropertyUsageByGroupOfResource}:{group}", () =>
               _resourceStatisticsRepository.GetNumberOfPropertyUsageByGroupOfResource(group), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            _logger.LogInformation("ResourceStatisticsService: Set CacheNumberOfPropertyUsageByGroupOfResource.... ");

        }
        /// <summary> 
        /// Returns how many properties of a given group are used by a resource. 
        /// </summary> 
        /// <param name="group">Identifier of a group</param> 
        /// <returns></returns> 
        public PropertyStatistics GetNumberOfPropertyUsageByGroupOfResource(Uri group)
        {
            Guard.IsValidUri(group);

            if (_cacheService.Exists($"{CacheKeys.Statistics.TotalNumberOfPropertyUsageByGroupOfResource}:{group}", () => _resourceStatisticsRepository.GetNumberOfPropertyUsageByGroupOfResource(null)) == false)
            {
                return new PropertyStatistics();
            }
           
            var counts = _cacheService.GetOrAdd($"{CacheKeys.Statistics.TotalNumberOfPropertyUsageByGroupOfResource}:{group}", () =>
               _resourceStatisticsRepository.GetNumberOfPropertyUsageByGroupOfResource(group), TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            return new PropertyStatistics(string.Empty, 0, counts);                        
        }

        public async Task CacheResourceTypeCharacteristics()
        {
            _cacheService.Delete(CacheKeys.Statistics.TotalResourceTypeCharacteristics, () => _resourceStatisticsRepository.GetResourceTypeCharacteristics(null));
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var cachedResourcesTypes = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalResourceTypeCharacteristics, () => _resourceStatisticsRepository.GetResourceTypeCharacteristics(types)
                             .OrderByDescending(t => t.Count)
                             .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            _logger.LogInformation("ResourceStatisticsService: Set CacheResourceTypeCharacteristics.... ");

        }

        public IList<PropertyCharacteristic> GetResourceTypeCharacteristics()
        {
            if (_cacheService.Exists(CacheKeys.Statistics.TotalResourceTypeCharacteristics, () => _resourceStatisticsRepository.GetResourceTypeCharacteristics(null)) == false)
            {
                return new List<PropertyCharacteristic>();
            }
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _cacheService.GetOrAdd(CacheKeys.Statistics.TotalResourceTypeCharacteristics, () => _resourceStatisticsRepository.GetResourceTypeCharacteristics(types)
                             .OrderByDescending(t => t.Count)
                             .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
        }

        public async Task CacheConsumerGroupCharacteristics()
        {
            _cacheService.Delete(CacheKeys.Statistics.TotalConsumerGroupCharacteristics, () => _resourceStatisticsRepository.GetConsumerGroupCharacteristics(null));
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);

            var cachedConsumerGroups = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalConsumerGroupCharacteristics, () => _resourceStatisticsRepository.GetConsumerGroupCharacteristics(types)
                             .OrderByDescending(t => t.Count)
                             .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            _logger.LogInformation($"ResourceStatisticsService: CacheConsumerGroupCharacteristics Count....{cachedConsumerGroups}");
            _logger.LogInformation("ResourceStatisticsService: Set CacheConsumerGroupCharacteristics.... ");
        }
        public IList<PropertyCharacteristic> GetConsumerGroupCharacteristics()
        {
            if (_cacheService.Exists(CacheKeys.Statistics.TotalConsumerGroupCharacteristics, () => _resourceStatisticsRepository.GetConsumerGroupCharacteristics(null)) == false)
            {
                return new List<PropertyCharacteristic>();
            }
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _cacheService.GetOrAdd(CacheKeys.Statistics.TotalConsumerGroupCharacteristics, () => _resourceStatisticsRepository.GetConsumerGroupCharacteristics(types)
                             .OrderByDescending(t => t.Count)
                             .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
        }

        public async Task CacheInformationClassificationCharacteristics()
        {
            _cacheService.Delete(CacheKeys.Statistics.TotalInformationClassificationCharacteristics, () => _resourceStatisticsRepository.GetInformationClassificationCharacteristics(null));
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var cachedInformationClassification = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalInformationClassificationCharacteristics, () => _resourceStatisticsRepository.GetInformationClassificationCharacteristics(types)
                            .OrderByDescending(t => t.Count)
                            .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            _logger.LogInformation("ResourceStatisticsService: Set CacheInformationClassificationCharacteristics.... ");
        }

        public IList<PropertyCharacteristic> GetInformationClassificationCharacteristics()
        {
            if (_cacheService.Exists(CacheKeys.Statistics.TotalInformationClassificationCharacteristics, () => _resourceStatisticsRepository.GetInformationClassificationCharacteristics(null)) == false)
            {
                return new List<PropertyCharacteristic>();
            }
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _cacheService.GetOrAdd(CacheKeys.Statistics.TotalInformationClassificationCharacteristics, () => _resourceStatisticsRepository.GetInformationClassificationCharacteristics(types)
                            .OrderByDescending(t => t.Count)
                            .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
        }

        public async Task CacheLifecycleStatusCharacteristics()
        {
            _cacheService.Delete(CacheKeys.Statistics.TotalLifecycleStatusCharacteristics, () => _resourceStatisticsRepository.GetLifecycleStatusCharacteristics(null));
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var cachedLifecycleStatuses = _cacheService.GetOrAdd(CacheKeys.Statistics.TotalLifecycleStatusCharacteristics, () => _resourceStatisticsRepository.GetLifecycleStatusCharacteristics(types)
                             .OrderByDescending(t => t.Count)
                             .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
            _logger.LogInformation("ResourceStatisticsService: Set CacheLifecycleStatusCharacteristics.... ");
        }

        public IList<PropertyCharacteristic> GetLifecycleStatusCharacteristics()
        {
            if (_cacheService.Exists(CacheKeys.Statistics.TotalLifecycleStatusCharacteristics, () => _resourceStatisticsRepository.GetLifecycleStatusCharacteristics(null)) == false)
            {
                return new List<PropertyCharacteristic>();
            }
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _cacheService.GetOrAdd(CacheKeys.Statistics.TotalLifecycleStatusCharacteristics, () => _resourceStatisticsRepository.GetLifecycleStatusCharacteristics(types)
                             .OrderByDescending(t => t.Count)
                             .ToList(),
                              TimeSpan.FromSeconds(defaultExpirationTime_4hours));
        }

        /// <summary> 
        /// Break the results down into areas. 
        /// </summary> 
        /// <param name="results"></param> 
        /// <param name="increment">Indicates the size of the area.</param> 
        /// <returns></returns> 
        private static IList<PropertyStatisticItem> CreateIncrementResultOfList(IList<int> results, int increment, string total = null)
        {
            var incrementedResults = new Dictionary<int, int>();

            var initialIncrement = increment;

            foreach (var result in results)
            {
                while (result > increment)
                {
                    increment += initialIncrement;
                }

                if (incrementedResults.TryGetValue(increment, out var value))
                {
                    incrementedResults[increment] = value + 1;
                }
                else
                {
                    incrementedResults.Add(increment, 1);
                }
            }

            return incrementedResults.Select(t => new PropertyStatisticItem(t.Key.ToString(), t.Value.ToString(), total)).ToList();
        }

        /// <summary> 
        /// Counts the amount of words in a text. 
        /// </summary> 
        /// <param name="txtToCount">Text to be analyzed.</param> 
        /// <returns>Amount of words</returns> 
        private static int WordCount(string txtToCount)
        {
            string pattern = @"\w+";
            Regex regex = new Regex(pattern);

            int CountedWords = regex.Matches(string.IsNullOrWhiteSpace(txtToCount) ? string.Empty : txtToCount).Count;

            return CountedWords;
        }

        private static string GenerateCacheKey(string cacheKey)
        {
            return $"{CacheKeys.CallingClass.ResourceStatisticsService}:{cacheKey}";
             
        }
    }
}
