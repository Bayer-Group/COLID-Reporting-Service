using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using COLID.Graph.Metadata.Constants;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.FunctionalTests;
using Microsoft.VisualBasic;
using VDS.RDF;
using Xunit;

namespace FunctionalTests.Controllers
{
    public class StatisticsControllerTests : IClassFixture<FunctionTestsFixture>
    {
        private readonly HttpClient _client;
        private readonly FunctionTestsFixture _factory;

        private readonly string _apiPath = "api/statistics";

        public StatisticsControllerTests(FunctionTestsFixture factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region GetTotalNumberOfResources
        /// <summary>
        /// Route GET api/statistics/resource/total
        /// </summary>
        [Fact]
        public async Task GetTotalNumberOfResources_Success()
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/total");

            // Assert
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(content);
            Assert.Equal("21", actualTestResult);
        }

        #endregion    

        #region GetNumberOfProperties

        // TODO: Error with InMemoryRepository
        /// <summary>
        /// Route GET api/statistics/resource/numberofproperties
        /// </summary>
        // [Fact]
        public async Task GetNumberOfProperties_Success()
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofproperties");

            // Assert
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<PropertyStatistics>(content);
            Assert.Equal(2, actualTestResult.Counts.Count);
        }

        #endregion

        #region GetNumberOfControlledVocabularySelection

        /// <summary>
        /// Route GET api/statistics/resource/controlledvocabularyselection
        /// </summary>
        [Fact]
        public async Task GetNumberOfControlledVocabularySelection_Success()
        {
            // Arrange
            var cv = HttpUtility.UrlEncode(Resource.HasConsumerGroup);

            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/controlledvocabularyselection?property={cv}");

            // Assert
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<PropertyStatistics>(content);
            Assert.Equal(2, actualTestResult.Counts.Count);
        }

        /// <summary>
        /// Route GET api/statistics/resource/controlledvocabularyselection
        /// </summary>
        [Theory]
        [InlineData("format_error")]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetNumberOfControlledVocabularySelection_Error_BadRequest_InvalidUri(string uri)
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/controlledvocabularyselection?property={uri}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        #endregion

        #region GetNumberOfResourcesInRelationToPropertyLength

        // TODO: Fix Inline data and add resources
        /// <summary>
        /// Route GET api/statistics/resource/numberofresourcesinrelationtopropertylength
        /// </summary>
        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(5, 1)]
        [InlineData(10, 1)]
        public async Task GetNumberOfResourcesInRelationToPropertyLength_Success(int increment, int expectedResults)
        {
            // Arrange
            var cv = HttpUtility.UrlEncode(Resource.HasLabel);

            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofresourcesinrelationtopropertylength?property={cv}&increment={increment}");

            // Assert
            //result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<PropertyStatistics>(content);
            Assert.Equal(expectedResults, actualTestResult.Counts.Count);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task GetNumberOfResourcesInRelationToPropertyLength_Error_BadRequest_InvalidIncrement(int increment)
        {
            // Arrange
            var cv = HttpUtility.UrlEncode(Resource.HasLabel);

            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofresourcesinrelationtopropertylength?property={cv}&increment={increment}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Theory]
        [InlineData("format_error")]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetNumberOfResourcesInRelationToPropertyLength_Error_BadRequest_InvalidPropertyUri(string property)
        {
            // Arrange
            var increment = 1;

            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofresourcesinrelationtopropertylength?property={property}&increment={increment}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(5, 1)]
        [InlineData(10, 1)]
        public async Task GetNumberOfVersionsOfResources_Success(int increment, int expectedResults)
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofversionsofresources?increment={increment}");

            // Assert
            //result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<PropertyStatistics>(content);
            Assert.Equal(expectedResults, actualTestResult.Counts.Count);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public async Task GetNumberOfVersionsOfResources_Error_BadRequest_InvalidIncrement(int increment)
        {
            // Arrange
            var cv = HttpUtility.UrlEncode(Resource.HasLabel);

            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofversionsofresources?increment={increment}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        #endregion

        #region GetNumberOfPropertyUsageByGroupOfResource
        [Fact]
        public async Task GetNumberOfPropertyUsageByGroupOfResource_Success()
        {
            // Arrange
            var group = new Uri("http://pid.bayer.com/kos/19050/LinkTypes");

            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofpropertyusagebygroup?group={group}");

            // Assert
            //result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<PropertyStatistics>(content);
            Assert.Equal(2, actualTestResult.Counts.Count);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("0")]
        public async Task GetNumberOfPropertyUsageByGroupOfResource_Error_BadRequest_InvalidIncrement(string group)
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/numberofpropertyusagebygroup?group={group}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        #endregion

        #region GetResourceTypeCharacteristics

        /// <summary>
        /// Route GET api/statistics/resource/characteristics/type
        /// </summary>
        [Fact]
        public async Task GetResourceTypeCharacteristics_Success()
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/characteristics/type");

            // Assert
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PropertyCharacteristic>>(content);
            //Assert.Equal(15, actualTestResult.FirstOrDefault(t => t.Key));
            //Assert.Equal(7, );
            //Assert.Equal(2, actualTestResult.Count);
        }

        #endregion

        #region GetConsumerGroupCharacteristics

        /// <summary>
        /// Route GET api/statistics/resource/characteristics/consumergroup
        /// </summary>
        [Fact]
        public async Task GetConsumerGroupCharacteristics_Success()
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/characteristics/consumergroup");

            // Assert
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PropertyCharacteristic>>(content);
            Assert.Equal(2, actualTestResult.Count);
        }

        #endregion

        #region GetInformationClassificationCharacteristics

        /// <summary>
        /// Route GET api/statistics/resource/characteristics/informationclassification
        /// </summary>
        [Fact]
        public async Task GetInformationClassificationCharacteristics_Success()
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/resource/characteristics/informationclassification");

            // Assert
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PropertyCharacteristic>>(content);
            Assert.Equal(1, actualTestResult.Count);
        }

        #endregion
    }
}
