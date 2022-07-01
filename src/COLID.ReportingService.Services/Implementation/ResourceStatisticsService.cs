using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using COLID.Common.Utilities;
using COLID.Graph.Metadata.Constants;
using COLID.Graph.Metadata.Services;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.Repositories.Interface;
using COLID.ReportingService.Services.Interface;

namespace COLID.ReportingService.Services.Implementation
{
    public class ResourceStatisticsService : IResourceStatisticsService
    {
        private readonly IResourceStatisticsRepository _resourceStatisticsRepository;
        private readonly IMetadataService _metadataService;

        public ResourceStatisticsService(IResourceStatisticsRepository resourceStatisticsRepository, IMetadataService metadataService)
        {
            _resourceStatisticsRepository = resourceStatisticsRepository;
            _metadataService = metadataService;
        }

        /// <summary>
        /// Returns the amount of all resources in the database. Draft and publish are evaluated as one resource only.
        /// </summary>
        /// <returns>Amount of properties</returns>
        public string GetTotalNumberOfResources()
        {
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _resourceStatisticsRepository.GetTotalNumberOfResources(types);
        }

        /// <summary>
        /// Returns an object containing a list of properties and how many resources this property has.
        /// </summary>
        /// <returns></returns>
        public PropertyStatistics GetNumberOfProperties()
        {
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var results = _resourceStatisticsRepository.GetNumberOfProperties(types);

            foreach (var result in results)
            {
                result.Total = _resourceStatisticsRepository.GetTotalNumberOfResourcesByPredicate(new Uri(result.Property));
            }

            return new PropertyStatistics("Amount of properties", 0, results);
        }

        /// <summary>
        /// Specifies the selection options for the given property key and their amount of uses.
        /// </summary>
        /// <param name="property">Property key</param>
        /// <returns></returns>
        public PropertyStatistics GetNumberOfControlledVocabularySelection(Uri property)
        {
            Guard.IsValidUri(property);
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _resourceStatisticsRepository.GetNumberOfControlledVocabularySelection(property, types);
        }

        /// <summary>
        /// Returns the amount of words for a specific property.
        /// </summary>
        /// <param name="property">Property key</param>
        /// <param name="increment">The increment indicates the steps in which the number is to be separated.</param>
        /// <returns></returns>
        public PropertyStatistics GetNumberOfResourcesInRelationToNumberOfPropertyWords(Uri property, int increment)
        {
            Guard.IsValidUri(property);
            Guard.IsGreaterThanZero(increment);

            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var results = _resourceStatisticsRepository.GetPropertyValuesOfAllResources(property, types);
            var countedResults = results.Item2.Select(t => WordCount(t)).ToList();
            var total = _resourceStatisticsRepository.GetTotalNumberOfResourcesByPredicate(property);

            return new PropertyStatistics(results.Item1, increment, CreateIncrementResultOfList(countedResults, increment, total));
        }

        /// <summary>
        /// Indicates how many reosources has how many versions. Only one resource per version chain is used for analysis, so that the other resources are not considered further.
        /// </summary>
        /// <param name="increment">The increment indicates the steps in which the number is to be separated.</param>
        /// <returns></returns>
        public PropertyStatistics GetNumberOfVersionsOfResources(int increment)
        {
            Guard.IsGreaterThanZero(increment);

            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            var results = _resourceStatisticsRepository.GetNumberOfVersionsOfResources(types);
            var total = _resourceStatisticsRepository.GetTotalNumberOfResources(types);

            return new PropertyStatistics("Amount of resource versions", increment, CreateIncrementResultOfList(results, increment, total));
        }

        /// <summary>
        /// Returns how many properties of a given group are used by a resource.
        /// </summary>
        /// <param name="group">Identifier of a group</param>
        /// <returns></returns>
        public PropertyStatistics GetNumberOfPropertyUsageByGroupOfResource(Uri group)
        {
            Guard.IsValidUri(group);

            var counts = _resourceStatisticsRepository.GetNumberOfPropertyUsageByGroupOfResource(group);

            return new PropertyStatistics(string.Empty, 0, counts);
        }

        public IList<PropertyCharacteristic> GetResourceTypeCharacteristics()
        {
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _resourceStatisticsRepository.GetResourceTypeCharacteristics(types)
                .OrderByDescending(t => t.Count)
                .ToList();
        }

        public IList<PropertyCharacteristic> GetConsumerGroupCharacteristics()
        {
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _resourceStatisticsRepository.GetConsumerGroupCharacteristics(types)
                .OrderByDescending(t => t.Count)
                .ToList();
        }

        public IList<PropertyCharacteristic> GetInformationClassificationCharacteristics()
        {
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _resourceStatisticsRepository.GetInformationClassificationCharacteristics(types)
                .OrderByDescending(t => t.Count)
                .ToList();
        }

        public IList<PropertyCharacteristic> GetLifecycleStatusCharacteristics()
        {
            var types = _metadataService.GetInstantiableEntityTypes(Resource.Type.FirstResouceType);
            return _resourceStatisticsRepository.GetLifecycleStatusCharacteristics(types)
                .OrderByDescending(t => t.Count)
                .ToList();
        }

        /// <summary>
        /// Break the results down into areas.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="increment">Indicates the size of the area.</param>
        /// <returns></returns>
        private IList<PropertyStatisticItem> CreateIncrementResultOfList(IList<int> results, int increment, string total = null)
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
        private int WordCount(string txtToCount)
        {
            string pattern = @"\w+";
            Regex regex = new Regex(pattern);

            int CountedWords = regex.Matches(string.IsNullOrWhiteSpace(txtToCount) ? string.Empty : txtToCount).Count;

            return CountedWords;
        }
    }
}
