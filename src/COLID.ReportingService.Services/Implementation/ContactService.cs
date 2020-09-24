using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COLID.Common.Utilities;
using COLID.Graph.Metadata.Constants;
using COLID.Graph.Metadata.Services;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.Repositories.Interface;
using COLID.ReportingService.Services.Interface;

namespace COLID.ReportingService.Services.Implementation
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly IRemoteRegistrationService _remoteRegistrationService;
        private readonly IMetadataService _metadataService;

        public ContactService(IContactRepository contactRepository, IRemoteRegistrationService remoteRegistrationService, IMetadataService metadataService)
        {
            _contactRepository = contactRepository;
            _remoteRegistrationService = remoteRegistrationService;
            _metadataService = metadataService;
        }

        public IEnumerable<string> GetContacts()
        {
            return _contactRepository
                .GetContacts()
                .Where(p => !string.IsNullOrWhiteSpace(p));
        }

        public async Task<IEnumerable<ColidEntryContactsCto>> GetContactReferencedEntries(string userEmailAddress)
        {
            Guard.ArgumentNotNullOrWhiteSpace(userEmailAddress, "userEmailAddress");
            
            var types = _metadataService.GetLeafEntityTypes(Resource.Type.FirstResouceType);
            var contactTypes = _metadataService.GetLeafEntityTypes(EnterpriseCore.Person);

            var entries = _contactRepository.GetContactReferencedEntries(userEmailAddress, types, contactTypes);
            var consumerGroups = await _remoteRegistrationService.GetConsumerGroups();

            entries = entries
                .Select(entry =>
                {
                    var cg = consumerGroups.FirstOrDefault(t => t.Id == entry.ConsumerGroup?.OriginalString);
                    if (cg != null && cg.Properties.TryGetValue(ConsumerGroup.HasContactPerson, out var contacts))
                    {
                        var consumerGroupContact = new ContactCto()
                        {
                            EmailAddress = contacts?.FirstOrDefault() as string,
                            TypeUri = new Uri(ConsumerGroup.HasContactPerson),
                            TypeLabel = "Consumer group contact"
                        };

                        entry.ConsumerGroupContact = consumerGroupContact;
                    }

                    return entry;
                });

            return entries;
        }
    }
}
