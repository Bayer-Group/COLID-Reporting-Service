using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COLID.Graph.Metadata.Services;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.Repositories.Interface;
using COLID.ReportingService.Services.Implementation;
using COLID.ReportingService.Services.Interface;
using Moq;
using UnitTests.Builder;
using Xunit;

namespace UnitTests.Services
{

    public class ContactServiceTests
    {
        private readonly IContactService _contactService;
        private readonly Mock<IContactRepository> _mockContactRepository;
        private readonly Mock<IRemoteRegistrationService> _mockRegService;
        private readonly Mock<IMetadataService> _mockMetadataService;

        public ContactServiceTests()
        {
            _mockContactRepository = new Mock<IContactRepository>();
            _mockRegService = new Mock<IRemoteRegistrationService>();
            _mockMetadataService = new Mock<IMetadataService>();

            SetUpContactRepository();
            SetUpRemoteRegistrationService();

            _contactService = new ContactService(_mockContactRepository.Object, _mockRegService.Object, _mockMetadataService.Object);
        }

        #region Setup Mocks
        private void SetUpRemoteRegistrationService()
        {
            var cgs = new ConsumerGroupBuilder().GenerateSampleDataList();
            _mockRegService.Setup(s => s.GetConsumerGroups()).ReturnsAsync(cgs);
        }

        private void SetUpContactRepository()
        {
            var contacts = new List<string>()
            {
                "simon.lansing.ext@bayer.com",
                "christian.kaubisch.ext@bayer.com",
                "megan.wilson@bayer.com",
                "superadmin@bayer.com",
                "another.user@bayer.com",
                null,
                string.Empty
            };
            _mockContactRepository.Setup(mock => mock.GetContacts()).Returns(contacts);

            var colidEntryContact = new ColidEntryContactBuilder().GenerateSampleData().Build();
            var colidEntryContactList = new List<ColidEntryContactsCto>() { colidEntryContact };
            _mockContactRepository.Setup(mock => mock.GetContactReferencedEntries(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Returns(colidEntryContactList);
        }
        #endregion

        [Fact]
        public void GetContacts_Should_InvokeContactRepositoryGetContacts_Once()
        {
            var contacts = _contactService.GetContacts();

            _mockContactRepository.Verify(x => x.GetContacts(), Times.Once);
            Assert.Equal(5, contacts.Count());
        }

        [Fact]
        public async Task GetContactReferencedEntries_Success()
        {
            // Arrange
            var email = "tim.odenthal.ext@bayer.com";
            var expectedResult = new ColidEntryContactBuilder().GenerateSampleData().FilterByEmail(email).Build();

            // Act
            var results = await  _contactService.GetContactReferencedEntries(email);

            // Assert
            _mockContactRepository.Verify(m => m.GetContactReferencedEntries(email, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()), Times.Once);

            var firstResult = results.FirstOrDefault();
            Assert.NotNull(firstResult);
            Assert.NotNull(firstResult.ConsumerGroupContact);
            Assert.Equal(expectedResult.Contacts.Count(), firstResult.Contacts.Count());
            Assert.All(firstResult.Contacts, cp => Assert.False(cp.EmailAddress == email));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetContactReferencedEntries_Should_ThrowError_IfEmailIsNullOrEmpty(string email)
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _contactService.GetContactReferencedEntries(email));
        }
    }
}
