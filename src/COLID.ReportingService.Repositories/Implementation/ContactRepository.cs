using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using COLID.Graph.Metadata.Constants;
using COLID.Graph.Metadata.Repositories;
using COLID.Graph.TripleStore.Extensions;
using COLID.Graph.TripleStore.Repositories;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.Repositories.Interface;
using Microsoft.Extensions.Configuration;
using VDS.RDF.Query;

namespace COLID.ReportingService.Repositories.Implementation
{
    public class ContactRepository : IContactRepository
    {
        private readonly ITripleStoreRepository _tripleStoreRepository;
        private readonly IMetadataGraphConfigurationRepository _metadataGraphConfigurationRepository;
        private static readonly string _basePath = Path.GetFullPath("appsettings.json");
        private static readonly string _filePath = _basePath.Substring(0, _basePath.Length - 16);
        private static IConfigurationRoot _configuration = new ConfigurationBuilder()
                         .SetBasePath(_filePath)
                        .AddJsonFile("appsettings.json")
                        .Build();
        public static readonly string _httpserviceUrl = _configuration.GetValue<string>("HttpServiceUrl");
        public ContactRepository(ITripleStoreRepository tripleStoreRepository, IMetadataGraphConfigurationRepository metadataGraphConfigurationRepository)
        {
            _tripleStoreRepository = tripleStoreRepository;
            _metadataGraphConfigurationRepository = metadataGraphConfigurationRepository;
        }

        public IEnumerable<string> GetContacts()
        {
            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                    @"SELECT DISTINCT ?contact
                      @fromResourceNamedGraph
                      @fromMetadataNamedGraph
                      WHERE {
                                ?range rdfs:subClassOf* <@httpServiceUrl>.
                                ?predicate rdfs:range ?range.
                                ?subject ?predicate ?contact.
                            }
                      ORDER BY ?contact"
            };

            var technicalGroups = new HashSet<string> { Resource.Groups.TechnicalInformation, Resource.Groups.InvisibleTechnicalInformation };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("technicalGroups", technicalGroups.JoinAsValuesList());
            parametrizedString.SetPlainLiteral("httpServiceUrl", _httpserviceUrl + "kos/19014/Person");

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var contacts = results
                .Select(res => res.GetNodeValuesFromSparqlResult("contact")?.Value);

            return contacts;
        }

        public IEnumerable<ColidEntryContactsCto> GetContactReferencedEntries(string userEmailAddress, IEnumerable<string> resourceTypes, IEnumerable<string> contactTypes)
        {
            if (!resourceTypes.Any())
            {
                return new List<ColidEntryContactsCto>();
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                    @"SELECT DISTINCT *
                      @fromResourceNamedGraph
                      @fromMetadataNamedGraph
                      WHERE {
                                Values ?contactType { @contactTypes }
                                ?predicate rdfs:range ?contactType.
                                ?shacl sh:path ?predicate.
                                OPTIONAL { ?shacl sh:group ?shaclGroup }
                                ?shacl sh:name ?predicateLabel.
                                { 
                                    Select ?subject 
                                    WHERE
                                    {
                                        Values ?type { @resourceTypes }
                                        ?subject a ?type.
                                        { 
                                            ?subject ?predicate @userEmailAddress.
                                        }
                                        UNION
                                        { 
                                            ?subject @distribution | @mainDistribution ?endpoint.
                                            ?endpoint ?predicate @userEmailAddress.
                                        } 
                                    }
                                }
                                { 
                                    ?subject ?predicate ?contact.
                                }
                                UNION
                                { 
                                    ?subject @distribution | @mainDistribution ?endpoint .
                                    ?endpoint ?predicate ?contact.
                                }
                                ?subject @hasPid ?pidUri.
                                ?subject @hasLabel ?label.
                                ?subject @hasConsumerGroup ?consumerGroup.
                                FILTER NOT EXISTS { ?subject  @hasPidEntryDraft ?draftResource }
                            }
                      ORDER BY ?subject
                     "
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());

            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());
            parametrizedString.SetPlainLiteral("contactTypes", contactTypes.JoinAsValuesList());

            parametrizedString.SetLiteral("userEmailAddress", userEmailAddress);
            parametrizedString.SetUri("hasLabel", new Uri(Resource.HasLabel));
            parametrizedString.SetUri("hasPid", new Uri(EnterpriseCore.PidUri));
            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasConsumerGroup", new Uri(Resource.HasConsumerGroup));
            parametrizedString.SetUri("distribution", new Uri(Resource.Distribution));
            parametrizedString.SetUri("mainDistribution", new Uri(Resource.MainDistribution));

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var groupedResults = results.GroupBy(res => res.GetNodeValuesFromSparqlResult("subject")?.Value);

            var contacts = groupedResults
                .Select(res =>
                {
                    var firstEntry = res.First();

                    var pidUriString = firstEntry.GetNodeValuesFromSparqlResult("pidUri")?.Value;
                    var pidUri = string.IsNullOrWhiteSpace(pidUriString) ? null : new Uri(pidUriString);

                    var consumerGroupString = firstEntry.GetNodeValuesFromSparqlResult("consumerGroup")?.Value;
                    var consumerGroup = string.IsNullOrWhiteSpace(consumerGroupString) ? null : new Uri(consumerGroupString);

                    var cep = new ColidEntryContactsCto()
                    {
                        Label = firstEntry.GetNodeValuesFromSparqlResult("label")?.Value,
                        PidUri = pidUri,
                        ConsumerGroup = consumerGroup,
                        Contacts = res
                        .Select(r =>
                        {
                            var roleId = r.GetNodeValuesFromSparqlResult("predicate")?.Value;
                            var contact = new ContactCto()
                            {
                                EmailAddress = r.GetNodeValuesFromSparqlResult("contact")?.Value,
                                TypeUri = string.IsNullOrWhiteSpace(roleId) ? null : new Uri(roleId),
                                TypeLabel = r.GetNodeValuesFromSparqlResult("predicateLabel")?.Value,
                                IsTechnicalContact = IsTechnicalGroup(r.GetNodeValuesFromSparqlResult("shaclGroup")?.Value)
                            };
                            return contact;
                        })
                    };

                    return cep;
                });


            return contacts;
        }

        private static bool IsTechnicalGroup(string group)
        {
            var technicalGroups = new HashSet<string>() { Resource.Groups.TechnicalInformation, Resource.Groups.InvisibleTechnicalInformation };
            return !string.IsNullOrWhiteSpace(group) && technicalGroups.Contains(group);
        }
    }
}
