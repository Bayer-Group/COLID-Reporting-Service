using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using COLID.ReportingService.Common.DataModels;

namespace COLID.ReportingService.Services.Interface
{
    public interface IResourceStatisticsService
    {
        /// <summary>
        /// Fetches the total number of resources within the resource and metadata graph from cache.
        /// </summary>
        /// <returns>The amount of resources as a string</returns>
        string GetTotalNumberOfResources();

        /// <summary>
        /// Determines the total number of controlled vocabularies.
        /// </summary>
        /// <param name="predicate">the controlled vocabulary key</param>
        /// <returns>a statistic item</returns>
        PropertyStatistics GetNumberOfControlledVocabularySelection(Uri predicate);

        /// <summary>
        /// Determines the total number of properties from redis cache.
        /// </summary>
        /// <returns>a statistic item</returns>
        PropertyStatistics GetNumberOfProperties();

        /// <summary>
        /// Returns the number of words for a specific property.
        /// </summary>
        /// <param name="property">Property key</param>
        /// <param name="increment">The increment indicates the steps in which the number is to be separated.</param>
        /// <returns>a statistic item</returns>
        PropertyStatistics GetNumberOfResourcesInRelationToNumberOfPropertyWords(Uri predicate, int increment);

        /// <summary>
        /// Determines the total number of properties.
        /// </summary>
        /// <param name="increment">The increment indicates the steps in which the number is to be separated</param>
        /// <returns>a statistic item</returns>
        PropertyStatistics GetNumberOfVersionsOfResources(int increment);

        /// <summary>
        /// Returns how many properties of a given group are used by a resource.
        /// </summary>
        /// <param name="group">Identifier of a group</param>
        /// <returns>a statistic item</returns>
        PropertyStatistics GetNumberOfPropertyUsageByGroupOfResource(Uri group);

        /// <summary>
        /// Returns the number of different expressions of colid types
        /// </summary>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetResourceTypeCharacteristics();

        /// <summary>
        /// Returns the number of different expressions of used consumer groups
        /// </summary>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetConsumerGroupCharacteristics();

        /// <summary>
        /// Returns the number of different expressions of used information classifications
        /// </summary>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetInformationClassificationCharacteristics();

        /// <summary>
        /// Returns the number of different expressions of resource lifecycle statuses
        /// </summary>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetLifecycleStatusCharacteristics();

        /// <summary>
        /// Caches the total number of resources from the resource and metadata graph to redis cache.
        /// </summary>
        Task CacheTotalNumberOfResources();

        /// <summary>
        /// Caches the total number of properties.
        /// </summary>
        Task CacheNumberOfProperties();

        /// <summary>
        /// Caches the selection options for the given property key and their amount of uses.
        /// </summary>
        Task CacheNumberOfResourcesInRelationToNumberOfPropertyWords(Uri property);

        /// <summary>
        /// Caches the resources version information.
        /// </summary>
        Task CacheNumberOfVersionsOfResources();

        /// <summary>
        /// Caches properties of a given group per resource.
        /// </summary>
        Task CacheNumberOfPropertyUsageByGroupOfResource(Uri group);
        
        /// <summary>
        /// Caches resource type properties for all resources.
        /// </summary>
        Task CacheResourceTypeCharacteristics();
        
        /// <summary>
        /// Caches consumer group properties for all resources.
        /// </summary>
        Task CacheConsumerGroupCharacteristics();

        /// <summary>
        /// Caches information classification properties for all resources.
        /// </summary>
        Task CacheInformationClassificationCharacteristics();

        /// <summary>
        /// Caches consumer group properties for all resources.
        /// </summary>
        Task CacheLifecycleStatusCharacteristics();


    }
}

