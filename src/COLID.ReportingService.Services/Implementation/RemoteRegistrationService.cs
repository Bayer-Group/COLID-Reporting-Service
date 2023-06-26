using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using COLID.Graph.TripleStore.DataModels.ConsumerGroups;
using COLID.Identity.Extensions;
using COLID.Identity.Services;
using COLID.ReportingService.Services.Configuration;
using COLID.ReportingService.Services.Interfaces;
using CorrelationId.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace COLID.ReportingService.Services.Implementation
{
    public class RemoteRegistrationService: IRemoteRegistrationService
    {
        private readonly CancellationToken _cancellationToken;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly ITokenService<ColidRegistrationServiceTokenOptions> _tokenService;
        private readonly bool _bypassProxy;
        private readonly string RegistrationServiceConsumerGroupApi;

        public RemoteRegistrationService(
            IHttpClientFactory clientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ICorrelationContextAccessor correlationContextAccessor,
            ITokenService<ColidRegistrationServiceTokenOptions> tokenService)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _tokenService = tokenService;
            _correlationContextAccessor = correlationContextAccessor;
            _cancellationToken = httpContextAccessor?.HttpContext?.RequestAborted ?? CancellationToken.None;
            _bypassProxy = _configuration.GetValue<bool>("BypassProxy");
            var serverUrl = _configuration.GetConnectionString("ColidRegistrationServiceUrl");
            RegistrationServiceConsumerGroupApi = $"{serverUrl}/consumerGroup";
        }

        public async Task<IEnumerable<ConsumerGroupResultDTO>> GetConsumerGroups()
        {
            using (var httpClient = (_bypassProxy ? _clientFactory.CreateClient("NoProxy") : _clientFactory.CreateClient()))
            {
                var response = await AquireTokenAndSendToRegistrationService(httpClient, HttpMethod.Get, $"{RegistrationServiceConsumerGroupApi}List/active", null);

                if (!response.IsSuccessStatusCode)
                {
                    throw new System.InvalidOperationException("Something went wrong while fetching consumer groups in RegistrationService");
                }

                var content = await response.Content.ReadAsStringAsync();
                var consumerGroups = JsonConvert.DeserializeObject<IList<ConsumerGroupResultDTO>>(content);

                return consumerGroups;
            }
        }

        private async Task<HttpResponseMessage> AquireTokenAndSendToRegistrationService(HttpClient httpClient, HttpMethod httpMethod, string endpointUrl, object requestBody)
        {
            var accessToken = await _tokenService.GetAccessTokenForWebApiAsync();
            var response = await httpClient.SendRequestWithOptionsAsync(httpMethod, endpointUrl,
                requestBody, accessToken, _cancellationToken, _correlationContextAccessor.CorrelationContext);
            return response;
        }
    }
}
