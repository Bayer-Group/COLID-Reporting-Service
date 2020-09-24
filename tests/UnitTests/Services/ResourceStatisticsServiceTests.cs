using System;
using System.Collections.Generic;
using COLID.ReportingService.Repositories.Interface;
using COLID.ReportingService.Services.Implementation;
using COLID.ReportingService.Services.Interface;
using Moq;
using Xunit;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.UnitTests.Builder;
using COLID.Graph.Metadata.Services;

namespace COLID.ReportingService.UnitTests.Services
{
    public class ResourceStatisticsServiceTests
    {
        private readonly IResourceStatisticsService _statisticsService;
        private readonly Mock<IResourceStatisticsRepository> _mockStatisticsRepository;
        private readonly Mock<IMetadataService> _mockMetadataService;

        private readonly PropertyStatistics _numberOfProperties;
        private readonly PropertyStatistics _propertyUsageByGroup;
        private readonly PropertyStatistics _cvSelection;

        public ResourceStatisticsServiceTests()
        {
            _mockStatisticsRepository = new Mock<IResourceStatisticsRepository>();
            _mockMetadataService = new Mock<IMetadataService>();

            // Init testdata
            _statisticsService = new ResourceStatisticsService(_mockStatisticsRepository.Object, _mockMetadataService.Object);
            
            _numberOfProperties = new PropertyStatisticsBuilder().GenerateSampleCountOfPropertiesData().Build();
            _propertyUsageByGroup = new PropertyStatisticsBuilder().GenerateSamplePropertyUsageByGroupData().Build();
            _cvSelection = new PropertyStatisticsBuilder().GenerateSampleControlledVocabularySelectionData().Build();

            _mockStatisticsRepository.Setup(s => s.GetNumberOfProperties(It.IsAny<List<string>>())).Returns(_numberOfProperties.Counts);
            _mockStatisticsRepository.Setup(s => s.GetNumberOfPropertyUsageByGroupOfResource(It.IsAny<Uri>())).Returns(_propertyUsageByGroup.Counts);
            _mockStatisticsRepository.Setup(s => s.GetNumberOfControlledVocabularySelection(It.IsAny<Uri>(), It.IsAny<List<string>>())).Returns(_cvSelection);
        }

        [Fact]
        public void GetTotalNumberOfResources_Should_InvokeUserGetTotalNumberOfResources_Once()
        {
            var items = _statisticsService.GetTotalNumberOfResources();

            _mockStatisticsRepository.Verify(x => x.GetTotalNumberOfResources(It.IsAny<List<string>>()), Times.Once);
        }

        [Fact]
        public void GetNumberOfProperties_Should_ReturnPropertyStatistics()
        {
            var results = _statisticsService.GetNumberOfProperties();

            _mockStatisticsRepository.Verify(x => x.GetNumberOfProperties(It.IsAny<List<string>>()), Times.Once);
            _mockStatisticsRepository.Verify(x => x.GetTotalNumberOfResourcesByPredicate(It.IsAny<Uri>()), Times.Exactly(results.Counts.Count));
            Assert.Equal(_numberOfProperties.Counts.Count, results.Counts.Count);
        }

        // TODO
        [Fact]
        public void GetNumberOfControlledVocabularySelection_Should_ReturnNumberOfControlledVocabulary()
        {
            var property = new Uri(Graph.Metadata.Constants.Resource.HasConsumerGroup);
            var results = _statisticsService.GetNumberOfControlledVocabularySelection(property);

            _mockStatisticsRepository.Verify(x => x.GetNumberOfControlledVocabularySelection(It.IsAny<Uri>(), It.IsAny<List<string>>()), Times.Once);
            Assert.Equal(_cvSelection.Counts.Count, results.Counts.Count);
        }

        [Fact]
        public void GetNumberOfControlledVocabularySelection_Should_ThrowException_IfUriIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _statisticsService.GetNumberOfControlledVocabularySelection(null));
        }

        [Fact]
        public void GetNumberOfControlledVocabularySelection_Should_ThrowException_IfUriIsEmpty()
        {
            Assert.Throws<UriFormatException>(() => _statisticsService.GetNumberOfControlledVocabularySelection(new Uri(string.Empty)));
        }

        [Fact]
        public void GetNumberOfControlledVocabularySelection_Should_ThrowException_IfUriFormatIsInvalid()
        {
            Assert.Throws<UriFormatException>(() => _statisticsService.GetNumberOfControlledVocabularySelection(new Uri("meeeeh")));
        }

        [Theory]
        [InlineData(1, 4)]
        [InlineData(2, 4)]
        [InlineData(5, 3)]
        [InlineData(10, 2)]
        public void GetNumberOfResourcesInRelationToNumberOfPropertyWords_Should_ReturnNumberOfControlledVocabulary(int increment, int expectedResults)
        {
            // Arrange
            var property = new Uri(COLID.Graph.Metadata.Constants.Resource.HasLabel);
            var words = new List<string>()
            {
                "Lorem ipsum dolor sit", // 4
                "Lorem ipsum dolor sit", // 4
                "Lorem ipsum dolor sit", // 4
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit", // 8 
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit", // 8 
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit", // 8 
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit", // 8 
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit", // 8 
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit", // 8
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum nisi augue, condimentum sit amet neque vitae", // 16
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum nisi augue, condimentum sit amet neque vitae, venenatis rhoncus nibh!", // 19
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum nisi augue, condimentum sit amet neque vitae, venenatis rhoncus nibh!", // 19
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum nisi augue, condimentum sit amet neque vitae, venenatis rhoncus nibh!", // 19
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum nisi augue, condimentum sit amet neque vitae, venenatis rhoncus nibh!", // 19
            };

            var wordTuple = new Tuple<string, IList<string>>("Label", words);
            _mockStatisticsRepository.Setup(s => s.GetPropertyValuesOfAllResources(property, It.IsAny<List<string>>())).Returns(wordTuple);
            _mockStatisticsRepository.Setup(s => s.GetTotalNumberOfResourcesByPredicate(property)).Returns("13");

            // Act
            var result = _statisticsService.GetNumberOfResourcesInRelationToNumberOfPropertyWords(property, increment);

            // Assert
            _mockStatisticsRepository.Verify(x => x.GetPropertyValuesOfAllResources(property, It.IsAny<List<string>>()), Times.Once);
            _mockStatisticsRepository.Verify(x => x.GetTotalNumberOfResourcesByPredicate(property), Times.Once);
            AssertPropertyStatisticsResult(result, wordTuple.Item1, increment, expectedResults);
        }

        [Fact]
        public void GetNumberOfResourcesInRelationToNumberOfPropertyWords_Should_ThrowException_IfIncrementIsZero()
        {
            Assert.Throws<ArgumentNullException>(() => _statisticsService.GetNumberOfResourcesInRelationToNumberOfPropertyWords(null, 0));
        }

        [Fact]
        public void GetNumberOfResourcesInRelationToNumberOfPropertyWords_Should_ThrowException_IfUriIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _statisticsService.GetNumberOfResourcesInRelationToNumberOfPropertyWords(null, 1));
        }

        [Fact]
        public void GetNumberOfResourcesInRelationToNumberOfPropertyWords_Should_ThrowException_IfUriIsEmpty()
        {
            Assert.Throws<UriFormatException>(() => _statisticsService.GetNumberOfResourcesInRelationToNumberOfPropertyWords(new Uri(string.Empty), 1));
        }

        [Fact]
        public void GetNumberOfResourcesInRelationToNumberOfPropertyWords_Should_ThrowException_IfUriFormatIsInvalid()
        {
            Assert.Throws<UriFormatException>(() => _statisticsService.GetNumberOfResourcesInRelationToNumberOfPropertyWords(new Uri("meeeeh"), 1));
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 8)]
        [InlineData(3, 7)]
        [InlineData(4, 6)]
        [InlineData(5, 4)]
        [InlineData(10, 3)]
        [InlineData(20, 2)]
        [InlineData(50, 1)]
        public void GetNumberOfVersionsOfResources_Should_ReturnPropertyStatistics(int increment, int expectedResults)
        {
            // Arrange
            var name = "Amount of resource versions";
            var list = new List<int>() { 1, 2, 3, 5, 11, 15, 17, 19, 20, 50 };
            _mockStatisticsRepository.Setup(s => s.GetNumberOfVersionsOfResources(It.IsAny<List<string>>())).Returns(list);

            // Act
            var result = _statisticsService.GetNumberOfVersionsOfResources(increment);

            // Assert
            _mockStatisticsRepository.Verify(x => x.GetNumberOfVersionsOfResources(It.IsAny<List<string>>()), Times.Once);
            AssertPropertyStatisticsResult(result, name, increment, expectedResults);
        }

        [Fact]
        public void GetNumberOfVersionsOfResources_Should_ThrowException_IfIncrementIsZero()
        {
            Assert.Throws<ArgumentException>(() => _statisticsService.GetNumberOfVersionsOfResources(0));
        }

        [Fact]
        public void GetNumberOfPropertyUsageByGroupOfResource_Should_ReturnPropertyStatistics()
        {
            // Arrange
            var group = new Uri("http://pid.bayer.com/kos/19050/LinkTypes");

            // Act
            var result = _statisticsService.GetNumberOfPropertyUsageByGroupOfResource(group);

            // Assert
            _mockStatisticsRepository.Verify(x => x.GetNumberOfPropertyUsageByGroupOfResource(group), Times.Once);
            AssertPropertyStatisticsResult(result, "", 0, 6);
        }

        [Fact]
        public void GetNumberOfPropertyUsageByGroupOfResource_Should_ThrowException_IfUriIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _statisticsService.GetNumberOfPropertyUsageByGroupOfResource(null));
        }

        [Fact]
        public void GetNumberOfPropertyUsageByGroupOfResource_Should_ThrowException_IfUriIsEmpty()
        {
            Assert.Throws<UriFormatException>(() => _statisticsService.GetNumberOfPropertyUsageByGroupOfResource(new Uri(string.Empty)));
        }

        [Fact]
        public void GetNumberOfPropertyUsageByGroupOfResource_Should_ThrowException_IfUriFormatIsInvalid()
        {
            Assert.Throws<UriFormatException>(() => _statisticsService.GetNumberOfPropertyUsageByGroupOfResource(new Uri("meeeeh")));
        }

        [Fact]
        public void GetConsumerGroupCharacteristics_Should_ReturnConsumerGroupCharacteristics()
        {
            // Arrange
            var characteristics = new PropertyCharacteristicListBuilder().GenerateConsumerGroupCharacteristics().Build();
            _mockStatisticsRepository.Setup(s => s.GetConsumerGroupCharacteristics(It.IsAny<IList<string>>())).Returns(characteristics);

            // Act
            var result = _statisticsService.GetConsumerGroupCharacteristics();

            // Assert
            _mockStatisticsRepository.Verify(x => x.GetConsumerGroupCharacteristics(It.IsAny<IList<string>>()), Times.Once);
            Assert.Equal(characteristics.Count, result.Count);
        }

        [Fact]
        public void GetResourceTypeCharacteristics_Should_ReturnResourceTypeCharacteristics()
        {
            // Arrange
            var characteristics = new PropertyCharacteristicListBuilder().GenerateResourceTypeCharacteristics().Build();
            _mockStatisticsRepository.Setup(s => s.GetResourceTypeCharacteristics(It.IsAny<IList<string>>())).Returns(characteristics);

            // Act
            var result = _statisticsService.GetResourceTypeCharacteristics();

            // Assert
            _mockStatisticsRepository.Verify(x => x.GetResourceTypeCharacteristics(It.IsAny<IList<string>>()), Times.Once);
            Assert.Equal(characteristics.Count, result.Count);
        }

        [Fact]
        public void GetInformationClassificationCharacteristics_Should_ReturnInformationClassificationCharacteristics()
        {
            // Arrange
            var characteristics = new PropertyCharacteristicListBuilder().GenerateInformationClassificationCharacteristics().Build();
            _mockStatisticsRepository.Setup(s => s.GetInformationClassificationCharacteristics(It.IsAny<IList<string>>())).Returns(characteristics);

            // Act
            var result = _statisticsService.GetInformationClassificationCharacteristics();

            // Assert
            _mockStatisticsRepository.Verify(x => x.GetInformationClassificationCharacteristics(It.IsAny<IList<string>>()), Times.Once);
            Assert.Equal(characteristics.Count, result.Count);
        }


        #region Helper
        private void AssertPropertyStatisticsResult(PropertyStatistics statistics, string name, int increment, int counts)
        {
            Assert.Equal(name, statistics.Name);
            Assert.Equal(increment, statistics.Increment);
            Assert.Equal(counts, statistics.Counts.Count);
        }
        #endregion
    }
}
