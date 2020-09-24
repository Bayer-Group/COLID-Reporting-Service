using System;
using System.Collections.Generic;
using System.Linq;
using COLID.Graph.Metadata.Constants;
using COLID.Graph.Metadata.Repositories;
using COLID.Graph.TripleStore.Extensions;
using COLID.ReportingService.Common.DataModels;
using COLID.ReportingService.Repositories.Interface;
using VDS.RDF.Query;
using COLID.Graph.TripleStore.Repositories;

namespace COLID.ReportingService.Repositories.Implementation
{
    public class ResourceStatisticsRepository : IResourceStatisticsRepository
    {
        private readonly ITripleStoreRepository _tripleStoreRepository;
        private readonly IMetadataGraphConfigurationRepository _metadataGraphConfigurationRepository;

        public ResourceStatisticsRepository(ITripleStoreRepository tripleStoreRepository, IMetadataGraphConfigurationRepository metadataGraphConfigurationRepository)
        {
            _tripleStoreRepository = tripleStoreRepository;
            _metadataGraphConfigurationRepository = metadataGraphConfigurationRepository;
        }

        /// <summary>
        /// Returns a list full of properties with the number of resources this propterty has.
        /// </summary>
        /// <returns></returns>
        public IList<PropertyStatisticItem> GetNumberOfProperties(IList<string> resourceTypes)
        {
            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                    @"SELECT ?predicate ?predicateName (count(distinct ?resource) as ?resources) ?minCount ?group
                      @fromResourceNamedGraph
                      @fromConsumerGroupNamedGraph
                      @fromMetadataNamedGraph
                      @fromMetdataShaclNamedGraph
                      WHERE {
                            Values ?type { @resourceTypes }
                            {
                            ?resource rdf:type ?type.
                            FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                            ?resource @hasPidUri ?pidUri .
                            ?resource ?predicate ?o.
                            ?shacl sh:path ?predicate.
                            ?shacl sh:name ?predicateName.
                            ?shacl sh:group ?group.
                            OPTIONAL { ?shacl sh:minCount ?minCount. }
                            FILTER ((str(?minCount) = @minCount || NOT exists { ?shacl sh:minCount ?minCount }) && ?group != @linkingGroup  && ?group != @distributionEndpoints )
                            } UNION
                            {
                            ?resource rdf:type ?type.
                            FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                            ?resource @mainDistribution | @distribution ?o
                            bind(@distribution as ?predicate)
                            OPTIONAL { ?shacl sh:path ?predicate.
                                       ?shacl sh:name ?predicateName.
                                     }
                             }}
                      GROUP BY ?predicate ?minCount ?predicateName ?group
                      ORDER BY ?resources"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetdataShaclNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasShaclConstraintsGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));
            parametrizedString.SetUri("distribution", new Uri(Resource.Distribution));
            parametrizedString.SetUri("mainDistribution", new Uri(Resource.MainDistribution));
            parametrizedString.SetUri("distributionEndpoints", new Uri(Resource.Groups.DistributionEndpoints));
            parametrizedString.SetLiteral("minCount", "0");
            parametrizedString.SetUri("linkingGroup", new Uri(Resource.Groups.LinkTypes));
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var countResults = results.Select(res =>
                new PropertyStatisticItem(res.GetNodeValuesFromSparqlResult("predicateName")?.Value, res.GetNodeValuesFromSparqlResult("resources")?.Value, res.GetNodeValuesFromSparqlResult("predicate")?.Value)
            );

            return countResults.ToList();
        }

        public PropertyStatistics GetNumberOfControlledVocabularySelection(Uri predicate, IList<string> resourceTypes)
        {
            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                   @"SELECT ?predicate ?predicateName ?label ?o (count(DISTINCT ?resource) as ?valueSum)
                      @fromResourceNamedGraph
                      @fromConsumerGroupNamedGraph
                      @fromMetadataNamedGraph
                      @fromMetdataShaclNamedGraph
                      WHERE
                      {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource  @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                          ?resource @predicate ?o.
                          ?resource ?predicate ?o .
                          ?o rdfs:label ?label .
                          ?shacl sh:path ?predicate.
                          ?shacl sh:name ?predicateName
                    }
                    GROUP BY ?o ?label ?predicateName ?predicate
                    "
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetdataShaclNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasShaclConstraintsGraph).JoinAsFromNamedGraphs());

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            parametrizedString.SetUri("predicate", predicate);

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var groupedResult = results.GroupBy(r => r.GetNodeValuesFromSparqlResult("predicateName")?.Value).FirstOrDefault();

            var total = GetTotalNumberOfResourcesByPredicate(predicate);

            return new PropertyStatistics
                (
                groupedResult.Key,
                0,
                groupedResult.Select(r =>
                    new PropertyStatisticItem(r.GetNodeValuesFromSparqlResult("label")?.Value, r.GetNodeValuesFromSparqlResult("valueSum")?.Value, r.GetNodeValuesFromSparqlResult("predicate")?.Value, total)).ToList()
            );
        }

        /// <summary>
        /// Returns the number of resources that can have the predicate
        /// </summary>
        /// <param name="predicate">Property key</param>
        /// <returns></returns>
        public string GetTotalNumberOfResourcesByPredicate(Uri predicate)
        {
            if (!predicate.IsValidBaseUri())
            {
                throw new FormatException("The request does not match the required format.");
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                  @"SELECT ?predicate (count(DISTINCT ?resource) as ?valueSum)
                      @fromResourceNamedGraph
                      @fromConsumerGroupNamedGraph
                      @fromMetadataNamedGraph
                      @fromMetdataShaclNamedGraph
                      @fromMetdataCoreNamedGraph
                      WHERE
                      {
                          @predicate rdfs:domain ?class .
                          BIND(@predicate as ?predicate).
                          ?resource rdf:type [rdfs:subClassOf* ?class]
                          FILTER NOT EXISTS { ?resource  @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                    }
                    GROUP BY ?predicate
                    "
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetdataShaclNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasShaclConstraintsGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetdataCoreNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasECOGraph).JoinAsFromNamedGraphs());

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));

            parametrizedString.SetUri("predicate", predicate);

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            return results.FirstOrDefault()?.GetNodeValuesFromSparqlResult("valueSum")?.Value;
        }

        public IList<PropertyCharacteristic> GetResourceTypeCharacteristics(IList<string> resourceTypes)
        {
            if (!resourceTypes.Any())
            {
                return new List<PropertyCharacteristic>();
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                   @"SELECT ?type ?label (count(?resource) as ?count)
                      @fromResourceNamedGraph
                      @fromMetadataNamedGraph
                      @fromMetdataCoreNamedGraph
                      WHERE
                      {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?type rdfs:label ?label.
                          FILTER(lang(?label) IN (@language , """"))
                      }
                      GROUP BY ?type ?label"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetdataCoreNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasECOGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());
            parametrizedString.SetLiteral("language", I18n.DefaultLanguage);

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var characteristics = results.Select(result => new PropertyCharacteristic()
            {
                Name = result.GetNodeValuesFromSparqlResult("label")?.Value,
                Key = result.GetNodeValuesFromSparqlResult("type")?.Value,
                Count = ParseStringToInt(result.GetNodeValuesFromSparqlResult("count")?.Value)

            }).ToList();

            return characteristics;
        }

        public IList<PropertyCharacteristic> GetConsumerGroupCharacteristics(IList<string> resourceTypes)
        {
            if (!resourceTypes.Any())
            {
                return new List<PropertyCharacteristic>();
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                   @"SELECT ?consumerGroup ?label (count(?resource) as ?count)
                      @fromResourceNamedGraph
                      @fromConsumerGroupNamedGraph
                      WHERE
                      {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasConsumerGroup ?consumerGroup.
                          ?consumerGroup rdfs:label ?label.
                      }
                      GROUP BY ?consumerGroup ?label"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasConsumerGroup", new Uri(Resource.HasConsumerGroup));

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var characteristics = results.Select(result => new PropertyCharacteristic()
            {
                Name = result.GetNodeValuesFromSparqlResult("label")?.Value,
                Key = result.GetNodeValuesFromSparqlResult("consumerGroup")?.Value,
                Count = ParseStringToInt(result.GetNodeValuesFromSparqlResult("count")?.Value)

            }).ToList();

            return characteristics;
        }

        public IList<PropertyCharacteristic> GetInformationClassificationCharacteristics(IList<string> resourceTypes)
        {
            if (!resourceTypes.Any())
            {
                return new List<PropertyCharacteristic>();
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                   @"SELECT ?informationClassification ?label (count(?resource) as ?count)
                      @fromResourceNamedGraph
                      @fromMetadataNamedGraph
                      WHERE
                      {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasInformationClassification ?informationClassification.
                          ?informationClassification rdfs:label ?label.
                      }
                      GROUP BY ?informationClassification ?label"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasInformationClassification", new Uri(Resource.HasInformationClassification));

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var characteristics = results.Select(result => new PropertyCharacteristic()
            {
                Name = result.GetNodeValuesFromSparqlResult("label")?.Value,
                Key = result.GetNodeValuesFromSparqlResult("informationClassification")?.Value,
                Count = ParseStringToInt(result.GetNodeValuesFromSparqlResult("count")?.Value)

            }).ToList();

            return characteristics;
        }

        private int ParseStringToInt(string str)
        {
            return int.TryParse(str, out var num) ? num : 0;
        }

        public string GetTotalNumberOfResources(IList<string> resourceTypes)
        {
            if (!resourceTypes.Any())
            {
                return "0";
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                    @"SELECT (COUNT(DISTINCT ?resource) AS ?resourceSum)
                      @fromResourceNamedGraph
                      WHERE {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource  @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                        }"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            return results.FirstOrDefault().GetNodeValuesFromSparqlResult("resourceSum")?.Value;
        }

        public Tuple<string, IList<string>> GetPropertyValuesOfAllResources(Uri predicate, IList<string> resourceTypes)
        {
            if (!predicate.IsValidBaseUri())
            {
                throw new FormatException("The request does not match the required format.");
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                   @"SELECT DISTINCT *
                      @fromResourceNamedGraph
                      @fromMetadataNamedGraph
                      @fromMetdataShaclNamedGraph
                      WHERE
                      {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                          OPTIONAL {
                          ?resource @predicate ?string.
                          ?shacl sh:path @predicate.
                          ?shacl sh:name ?predicateName
                          }
                        FILTER(lang(?predicateName) IN (@language , """"))
                      }
                      ORDER BY strlen(str(?string))"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetdataShaclNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasShaclConstraintsGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));
            parametrizedString.SetUri("predicate", predicate);
            parametrizedString.SetLiteral("language", I18n.DefaultLanguage);
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var label = results.LastOrDefault().GetNodeValuesFromSparqlResult("predicateName")?.Value;
            var result = results.Select(res => res.GetNodeValuesFromSparqlResult("string")?.Value).ToList();

            return new Tuple<string, IList<string>>(label, result);
        }

        public IList<int> GetNumberOfVersionsOfResources(IList<string> resourceTypes)
        {
            var parametrizedString = new SparqlParameterizedString
            {
                CommandText = @"
                      SELECT (count(distinct ?laterVersion) as ?laterVersions)
                      @fromResourceNamedGraph
                      WHERE
                      {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource  @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                          ?resource @hasLaterVersion+ ?laterVersion.
                          FILTER NOT EXISTS { ?previousVersion @hasLaterVersion ?resource }
                    }
                    group by ?resource
                    order by ?laterVersions"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));
            parametrizedString.SetUri("hasLaterVersion", new Uri(Resource.HasLaterVersion));
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            return results.Select(res => int.Parse(res.GetNodeValuesFromSparqlResult("laterVersions")?.Value)).ToList();
        }

        public IList<PropertyStatisticItem> GetNumberOfPropertyUsageByGroupOfResource(Uri group)
        {
            if (!group.IsValidBaseUri())
            {
                throw new FormatException(Messages.Identifier.IncorrectIdentifierFormat);
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText = @"
                      Select ?links (count(?resource) as ?resources)
                      @fromResourceNamedGraph
                      @fromMetadataNamedGraph
                      @fromMetdataShaclNamedGraph
                      where {
                             SELECT ?resource (count(?link) as ?links)

                             WHERE
                             {{
                          ?resource rdf:type [rdfs:subClassOf+ pid3:PID_Concept]
                          FILTER NOT EXISTS { ?resource  @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                          ?resource ?predicate ?link.
                          #?link ?predicate ?resource.
                          ?shacl sh:path ?predicate.
                          ?shacl sh:group @group
                             }

                          UNION {
                          ?resource rdf:type [rdfs:subClassOf+ pid3:PID_Concept]
                          FILTER NOT EXISTS { ?resource  @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                          ?resource ?predicate ?nonlink.
                          FILTER NOT EXISTS {
                          ?shacl sh:path ?predicate.
                          ?shacl sh:group @group }
                            }}
                        group by ?resource
                           }
                    group by ?links
                    ORDER BY ?links"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetdataShaclNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasShaclConstraintsGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));

            parametrizedString.SetUri("group", group);

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);
            return results.Select(res => new PropertyStatisticItem(res.GetNodeValuesFromSparqlResult("links")?.Value, res.GetNodeValuesFromSparqlResult("resources")?.Value, null)).ToList();
        }
    }
}
