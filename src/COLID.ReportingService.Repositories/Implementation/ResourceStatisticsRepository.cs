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
                      @fromResourceDraftGraph
                      @fromConsumerGroupNamedGraph
                      @fromMetadataNamedGraph
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
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
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
                      @fromResourceDraftGraph
                      @fromConsumerGroupNamedGraph
                      @fromMetadataNamedGraph
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
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());

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
                      @fromResourceDraftGraph
                      @fromConsumerGroupNamedGraph
                      @fromMetadataNamedGraph
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
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());

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
                   @"SELECT ?type ?label (count(?resource) as ?count) (count(?resourceDraft) as ?resourceDraftCount) 
                      (count(?resourcePublished) as ?resourcePublishedCount)
                      @fromResourceNamedGraph
                      @fromResourceDraftGraph
                      @fromMetadataNamedGraph
                      WHERE
                      {
                          {Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?type rdfs:label ?label.
                          FILTER(lang(?label) IN (@language , """"))}
			           UNION
                          {Values ?type { @resourceTypes }
                          ?resourceDraft rdf:type ?type.
                          FILTER NOT EXISTS { ?resourceDraft @hasPidEntryDraft ?draftResource. }.
                          ?type rdfs:label ?label.
                          ?resourceDraft @hasEntryLifecycleStatus @draftStatus. 
                          FILTER(lang(?label) IN (@language , """"))}
                       UNION
                          {Values ?type { @resourceTypes }
                          ?resourcePublished rdf:type ?type.
                          FILTER NOT EXISTS { ?resourcePublished @hasPidEntryDraft ?draftResource. }.
                          ?type rdfs:label ?label.
                          ?resourcePublished @hasEntryLifecycleStatus @publishedStatus.
                          FILTER(lang(?label) IN (@language , """"))}
                      }
                      GROUP BY ?type ?label"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());
            parametrizedString.SetLiteral("language", I18n.DefaultLanguage);

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasEntryLifecycleStatus", new Uri(Resource.HasEntryLifecycleStatus));
            parametrizedString.SetUri("draftStatus", new Uri(Resource.ColidEntryLifecycleStatus.Draft));
            parametrizedString.SetUri("publishedStatus", new Uri(Resource.ColidEntryLifecycleStatus.Published));
            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var characteristics = results.Select(result => new PropertyCharacteristic()
            {
                Name = result.GetNodeValuesFromSparqlResult("label")?.Value,
                Key = result.GetNodeValuesFromSparqlResult("type")?.Value,
                Count = ParseStringToInt(result.GetNodeValuesFromSparqlResult("count")?.Value),
                DraftCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourceDraftCount")?.Value),
                PublishedCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourcePublishedCount")?.Value)
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
                   @"SELECT ?consumerGroup ?label (count(?resource) as ?count) (count(?resourceDraft) as ?resourceDraftCount) 
                      (count(?resourcePublished) as ?resourcePublishedCount)
                      @fromResourceNamedGraph
                      @fromResourceDraftGraph
                      @fromConsumerGroupNamedGraph
                      WHERE
                      {
                          {Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasConsumerGroup ?consumerGroup.
                          ?consumerGroup rdfs:label ?label.}
                      UNION
                          {Values ?type { @resourceTypes }
                          ?resourceDraft rdf:type ?type.
                          FILTER NOT EXISTS { ?resourceDraft @hasPidEntryDraft ?draftResource. }.
                          ?resourceDraft @hasConsumerGroup ?consumerGroup.
                          ?consumerGroup rdfs:label ?label.
                          ?resourceDraft @hasEntryLifecycleStatus @draftStatus. }
                       UNION
                          {Values ?type { @resourceTypes }
                          ?resourcePublished rdf:type ?type.
                          FILTER NOT EXISTS { ?resourcePublished @hasPidEntryDraft ?draftResource. }.
                          ?resourcePublished @hasConsumerGroup ?consumerGroup.
                          ?consumerGroup rdfs:label ?label.
                          ?resourcePublished @hasEntryLifecycleStatus @publishedStatus. }
                      }
                      GROUP BY ?consumerGroup ?label"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromConsumerGroupNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasConsumerGroupGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasConsumerGroup", new Uri(Resource.HasConsumerGroup));
            parametrizedString.SetUri("hasEntryLifecycleStatus", new Uri(Resource.HasEntryLifecycleStatus));
            parametrizedString.SetUri("draftStatus", new Uri(Resource.ColidEntryLifecycleStatus.Draft));
            parametrizedString.SetUri("publishedStatus", new Uri(Resource.ColidEntryLifecycleStatus.Published));
            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var characteristics = results.Select(result => new PropertyCharacteristic()
            {
                Name = result.GetNodeValuesFromSparqlResult("label")?.Value,
                Key = result.GetNodeValuesFromSparqlResult("consumerGroup")?.Value,
                Count = ParseStringToInt(result.GetNodeValuesFromSparqlResult("count")?.Value),
                DraftCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourceDraftCount")?.Value),
                PublishedCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourcePublishedCount")?.Value)
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
                   @"SELECT ?informationClassification ?label (count(?resource) as ?count) (count(?resourceDraft) as ?resourceDraftCount) 
                      (count(?resourcePublished) as ?resourcePublishedCount)
                      @fromResourceNamedGraph
                      @fromResourceDraftGraph
                      @fromMetadataNamedGraph
                      WHERE
                      {
                          {Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasInformationClassification ?informationClassification.
                          ?informationClassification rdfs:label ?label.}
                       UNION
                          {Values ?type { @resourceTypes }
                          ?resourceDraft rdf:type ?type.
                          FILTER NOT EXISTS { ?resourceDraft @hasPidEntryDraft ?draftResource. }.
                          ?resourceDraft @hasInformationClassification ?informationClassification.
                          ?informationClassification rdfs:label ?label.
                          ?resourceDraft @hasEntryLifecycleStatus @draftStatus. }
                       UNION
                          {Values ?type { @resourceTypes }
                          ?resourcePublished rdf:type ?type.
                          FILTER NOT EXISTS { ?resourcePublished @hasPidEntryDraft ?draftResource. }.
                          ?resourcePublished @hasInformationClassification ?informationClassification.
                          ?informationClassification rdfs:label ?label.
                          ?resourcePublished @hasEntryLifecycleStatus @publishedStatus. }
                      }
                      GROUP BY ?informationClassification ?label"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasInformationClassification", new Uri(Resource.HasInformationClassification));
            parametrizedString.SetUri("hasEntryLifecycleStatus", new Uri(Resource.HasEntryLifecycleStatus));
            parametrizedString.SetUri("draftStatus", new Uri(Resource.ColidEntryLifecycleStatus.Draft));
            parametrizedString.SetUri("publishedStatus", new Uri(Resource.ColidEntryLifecycleStatus.Published));
            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var characteristics = results.Select(result => new PropertyCharacteristic()
            {
                Name = result.GetNodeValuesFromSparqlResult("label")?.Value,
                Key = result.GetNodeValuesFromSparqlResult("informationClassification")?.Value,
                Count = ParseStringToInt(result.GetNodeValuesFromSparqlResult("count")?.Value),
                DraftCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourceDraftCount")?.Value),
                PublishedCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourcePublishedCount")?.Value)
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
                      @fromResourceDraftGraph
                      WHERE {
                          Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource  @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasPidUri ?pidUri .
                        }"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
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
                      @fromResourceDraftGraph
                      @fromMetadataNamedGraph
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
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
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
                      @fromResourceDraftGraph
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
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
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
                      @fromResourceDraftGraph
                      @fromMetadataNamedGraph
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
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasPidUri", new Uri(EnterpriseCore.PidUri));

            parametrizedString.SetUri("group", group);

            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);
            return results.Select(res => new PropertyStatisticItem(res.GetNodeValuesFromSparqlResult("links")?.Value, res.GetNodeValuesFromSparqlResult("resources")?.Value, null)).ToList();
        }

        public IList<PropertyCharacteristic> GetLifecycleStatusCharacteristics(IList<string> resourceTypes)
        {
            if (!resourceTypes.Any())
            {
                return new List<PropertyCharacteristic>();
            }

            var parametrizedString = new SparqlParameterizedString
            {
                CommandText =
                   @"SELECT ?lifecycleStatus ?label (count(?resource) as ?count) (count(?resourceDraft) as ?resourceDraftCount) 
                      (count(?resourcePublished) as ?resourcePublishedCount)
                      @fromResourceNamedGraph
                      @fromResourceDraftGraph
                      @fromMetadataNamedGraph
                      WHERE
                      {
                          {Values ?type { @resourceTypes }
                          ?resource rdf:type ?type.
                          FILTER NOT EXISTS { ?resource @hasPidEntryDraft ?draftResource. }.
                          ?resource @hasLifecycleStatus ?lifecycleStatus.
                          ?lifecycleStatus rdfs:label ?label.
                          ?resource @hasEntryLifecycleStatus ?resourceStatus.}
                       UNION
                          {Values ?type { @resourceTypes }
                          ?resourceDraft rdf:type ?type.
                          FILTER NOT EXISTS { ?resourceDraft @hasPidEntryDraft ?draftResource. }.
                          ?resourceDraft @hasLifecycleStatus ?lifecycleStatus.
                          ?lifecycleStatus rdfs:label ?label.
                          ?resourceDraft @hasEntryLifecycleStatus @draftStatus. }
                       UNION
                          {Values ?type { @resourceTypes }
                          ?resourcePublished rdf:type ?type.
                          FILTER NOT EXISTS { ?resourcePublished @hasPidEntryDraft ?draftResource. }.
                          ?resourcePublished @hasLifecycleStatus ?lifecycleStatus.
                          ?lifecycleStatus rdfs:label ?label.
                          ?resourcePublished @hasEntryLifecycleStatus @publishedStatus. }
                      }
                      GROUP BY ?lifecycleStatus ?label"
            };

            parametrizedString.SetPlainLiteral("fromResourceNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromResourceDraftGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasResourcesDraftGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("fromMetadataNamedGraph", _metadataGraphConfigurationRepository.GetGraphs(MetadataGraphConfiguration.HasMetadataGraph).JoinAsFromNamedGraphs());
            parametrizedString.SetPlainLiteral("resourceTypes", resourceTypes.JoinAsValuesList());

            parametrizedString.SetUri("hasPidEntryDraft", new Uri(Resource.HasPidEntryDraft));
            parametrizedString.SetUri("hasLifecycleStatus", new Uri(Resource.LifecycleStatus));
            parametrizedString.SetUri("hasEntryLifecycleStatus", new Uri(Resource.HasEntryLifecycleStatus));
            parametrizedString.SetUri("draftStatus", new Uri(Resource.ColidEntryLifecycleStatus.Draft));
            parametrizedString.SetUri("publishedStatus", new Uri(Resource.ColidEntryLifecycleStatus.Published));


            var results = _tripleStoreRepository.QueryTripleStoreResultSet(parametrizedString);

            var characteristics = results.Select(result => new PropertyCharacteristic()
            {
                Name = result.GetNodeValuesFromSparqlResult("label")?.Value,
                Key = result.GetNodeValuesFromSparqlResult("lifecycleStatus")?.Value,
                Count = ParseStringToInt(result.GetNodeValuesFromSparqlResult("count")?.Value),
                DraftCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourceDraftCount")?.Value),
                PublishedCount = ParseStringToInt(result.GetNodeValuesFromSparqlResult("resourcePublishedCount")?.Value)
            }).ToList();

            return characteristics;
        }
    }
}
