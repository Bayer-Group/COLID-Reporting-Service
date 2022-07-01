using System;
using System.Collections.Generic;
using System.Text;
using COLID.ReportingService.Common.DataModels;

namespace COLID.ReportingService.Services.Interface
{
    public interface IResourceStatisticsService
    {
        /// <summary>
        /// Determines the total number of resources within the resource and metadata graph.
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
        /// Determines the total number of properties.
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
    }
}

