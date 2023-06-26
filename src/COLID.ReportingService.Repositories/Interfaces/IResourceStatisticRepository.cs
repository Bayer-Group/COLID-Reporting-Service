using System;
using System.Collections.Generic;
using System.Text;
using COLID.ReportingService.Common.DataModels;

namespace COLID.ReportingService.Repositories.Interfaces
{
    /// <summary>
    /// Repository to handle all resource statistic related operations.
    /// </summary>
    public interface IResourceStatisticsRepository
    {
        /// <summary>
        /// Determines the total number of resources within the resource and metadata graph.
        /// </summary>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>The amount of resources as a string</returns>
        string GetTotalNumberOfResources(IList<string> resourceTypes);

        /// <summary>
        /// Determines the total number of resources, filtered by the given property key.
        /// </summary>
        /// <param name="predicate">the property key</param>
        /// <returns>The amount of resources as a string</returns>
        string GetTotalNumberOfResourcesByPredicate(Uri predicate);

        /// <summary>
        /// Determines the total number of controlled vocabularies.
        /// </summary>
        /// <param name="predicate">the controlled vocabulary key</param>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>a statistic item</returns>
        PropertyStatistics GetNumberOfControlledVocabularySelection(Uri predicate, IList<string> resourceTypes);

        /// <summary>
        /// Determines the total number of properties.
        /// </summary>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>a list of statistic items</returns>
        IList<PropertyStatisticItem> GetNumberOfProperties(IList<string> resourceTypes);

        /// <summary>
        /// Determines all property values for all resources, filteres by the predicate.
        /// </summary>
        /// <param name="predicate">the predicate to consider</param>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>a label and result as a tuple</returns>
        Tuple<string, IList<string>> GetPropertyValuesOfAllResources(Uri predicate, IList<string> resourceTypes);

        /// <summary>
        /// Determines the total number of properties.
        /// </summary>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>a cumulated list of versions for resources</returns>
        IList<int> GetNumberOfVersionsOfResources(IList<string> resourceTypes);

        /// <summary>
        /// Returns how many properties of a given group are used by a resource.
        /// </summary>
        /// <param name="group">the resource group</param>
        /// <returns>a list of statistic items</returns>
        IList<PropertyStatisticItem> GetNumberOfPropertyUsageByGroupOfResource(Uri group);

        /// <summary>
        /// Returns the number of different expressions of colid types
        /// </summary>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetResourceTypeCharacteristics(IList<string> resourceTypes);

        /// <summary>
        /// Returns the number of different expressions of used consumer groups
        /// </summary>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetConsumerGroupCharacteristics(IList<string> resourceTypes);

        /// <summary>
        /// Returns the number of different expressions of used information classifications
        /// </summary>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetInformationClassificationCharacteristics(IList<string> resourceTypes);

        /// <summary>
        /// Returns the number of different expressions of resource lifecycle statuses
        /// </summary>
        /// <param name="resourceTypes">the resource type list to count</param>
        /// <returns>list of expression counts</returns>
        IList<PropertyCharacteristic> GetLifecycleStatusCharacteristics(IList<string> resourceTypes);

        /// <summary>
        /// Returns property list against provided resource Types.
        /// </summary>
        /// <param name="resourceTypes"></param>
        /// <returns></returns>
        IList<PropertyMetadata> GetAllPropertiesByResourceTypes(IList<string> resourceTypes);

        /// <summary>
        /// Returns count of usage of each property
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        IList<PropertyUsage> GetUsageOfProperties(IList<string> properties);
    }
}
