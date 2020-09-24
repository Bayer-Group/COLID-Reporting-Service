using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.FunctionalTests;
using Xunit;
using Xunit.Abstractions;

namespace FunctionalTests.Controllers
{
    public class ContactControllerTests : IClassFixture<FunctionTestsFixture>
    {
        private readonly HttpClient _client;
        private readonly FunctionTestsFixture _factory;
        private readonly ITestOutputHelper _output;

        private readonly string _apiPath = "api/contact";

        public ContactControllerTests(FunctionTestsFixture factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _output = output;
        }

        /// <summary>
        /// Route GET api/contacts
        /// </summary>
        [Fact]
        public async Task GetContacts_Success()
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}");

            // Assert
            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            _output.WriteLine(content);
            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<string>>(content);
            Assert.Equal(5, actualTestResult.Count());
        }

        /// <summary>
        /// Route GET api/contacts/{id}/colidEntries
        /// </summary>
        [Theory]
        [InlineData("christian.kaubisch.ext@bayer.com", 20)]
        [InlineData("simon.lansing.ext@bayer.com", 6)]
        [InlineData("tim.odenthal.ext@bayer.com", 3)]
        public async Task GetContactReferencedEntries_Success(string email, int expectedResults)
        {
            // Act
            var encodedEmail = HttpUtility.UrlEncode(email);
            var result = await _client.GetAsync($"{_apiPath}/{encodedEmail}/colidEntries");

            // Assert
            var content = await result.Content.ReadAsStringAsync();
            _output.WriteLine(content);
            result.EnsureSuccessStatusCode();

            var actualTestResult = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<ColidEntryContactsCto>>(content);
            Assert.Equal(expectedResults, actualTestResult.Count());
        }
        [Theory]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetContactReferencedEntries_Error_BadRequest_EmptyEmail(string email)
        {
            // Act
            var result = await _client.GetAsync($"{_apiPath}/{email}/colidEntries");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            var content = await result.Content.ReadAsStringAsync();
            _output.WriteLine(content);
        }
    }
}
