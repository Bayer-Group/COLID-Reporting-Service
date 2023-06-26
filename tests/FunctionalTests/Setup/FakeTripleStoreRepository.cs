using System;
using System.Collections.Generic;
using COLID.Graph.TripleStore.Repositories; 
using COLID.Graph.TripleStore.Transactions; 
using Microsoft.Extensions.Logging; 
using Moq; 
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Update;
using VDS.RDF.Writing;

namespace COLID.ReportingService.FunctionalTests.Setup
{
    public class FakeTripleStoreRepository : ITripleStoreRepository, ICommitable
    {
        private readonly VDS.RDF.TripleStore _store;

        private InMemoryDataset _dataset;

        private ITripleStoreTransaction _transaction;

        private readonly Mock<ILogger<TripleStoreTransaction>> _mockLogger; 

        public FakeTripleStoreRepository(Dictionary<string, string> graphs)
        {
            _store = CreateNewTripleStore(graphs);
            _dataset = new InMemoryDataset(_store);
            _mockLogger = new Mock<ILogger<TripleStoreTransaction>>();
        }

        public SparqlResultSet QueryTripleStoreResultSet(SparqlParameterizedString parameterizedString)
        {
            var processor = new LeviathanQueryProcessor(_dataset);

            var sparqlParser = new SparqlQueryParser();

            AddAllPidNamespaces(parameterizedString);

            var query = sparqlParser.ParseFromString(parameterizedString);

            Object results = processor.ProcessQuery(query);
            //Object results = _store.ExecuteQuery(query);

            if (results is SparqlResultSet rset)
            {
                //Print out the Results
                return rset;
            }

            return null;
        }

        public IGraph QueryTripleStoreGraphResult(SparqlParameterizedString queryString)
        {
            var processor = new LeviathanQueryProcessor(_dataset);

            var sparqlParser = new SparqlQueryParser();

            AddAllPidNamespaces(queryString);

            var query = sparqlParser.ParseFromString(queryString);

            Object results = processor.ProcessQuery(query);
            //Object results = _store.ExecuteQuery(query);

            if (results is IGraph)
            {
                //Print out the Results
                IGraph rset = (VDS.RDF.Graph)results;
                return rset;
            }

            return null;
        }

        public string QueryTripleStoreRaw(SparqlParameterizedString queryString)
        {
            var processor = new LeviathanQueryProcessor(_dataset);

            var sparqlParser = new SparqlQueryParser();

            AddAllPidNamespaces(queryString);

            var query = sparqlParser.ParseFromString(queryString);

            //Object results = _store.ExecuteQuery(query);
            Object results = processor.ProcessQuery(query);

            if (results is IGraph)
            {
                IGraph g = (IGraph)results;
                return ConvertGraphToString(g);
            }

            throw new System.Exception("Execute failed");
        }

        private string ConvertGraphToString(IGraph graph)
        {
            var turtleWriter = new CompressingTurtleWriter();
            var sw = new System.IO.StringWriter();
            turtleWriter.Save(graph, sw);
            var data = sw.ToString();
            return data;
        }

        private VDS.RDF.TripleStore CreateNewTripleStore(IDictionary<string, string> graphs)
        {
            var store = new VDS.RDF.TripleStore();

            foreach (var graph in graphs)
            {
                var g = new VDS.RDF.Graph();

                g.BaseUri = new Uri(graph.Value);

                var ttlparser = new TurtleParser();
                ttlparser.Load(g, AppDomain.CurrentDomain.BaseDirectory + $"Setup/Graphs/{graph.Key}");
                store.Add(g);
            };

            // TODO: Check if usesGraph is in graphGraph
            return store;
        }

        public void UpdateTripleStore(SparqlParameterizedString updateString)
        {
            if (_transaction != null)
            {
                _transaction.AddUpdateString(updateString);
            }
            else
            {
                var processor = new LeviathanUpdateProcessor(_dataset);
                var sparqlParser = new SparqlUpdateParser();

                AddAllPidNamespaces(updateString);

                var query = sparqlParser.ParseFromString(updateString);
                processor.ProcessCommandSet(query);
            }
        }

        public void Commit(SparqlParameterizedString updateTasks)
        {
            if (updateTasks != null)
            {
                var processor = new LeviathanUpdateProcessor(_dataset);
                var sparqlParser = new SparqlUpdateParser();

                AddAllPidNamespaces(updateTasks);

                var query = sparqlParser.ParseFromString(updateTasks);
                processor.ProcessCommandSet(query);
            }
        }

        public ITripleStoreTransaction CreateTransaction()
        {
            _transaction = new TripleStoreTransaction(this, _mockLogger.Object);
            return _transaction;
        }

        private static void AddAllPidNamespaces(SparqlParameterizedString sparql)
        {
            foreach (var prefix in SparqlUtil.SparqlPrefixes)
            {
                if (!sparql.Namespaces.HasNamespace(prefix.ShortPrefix))
                {
                    sparql.Namespaces.AddNamespace(prefix.ShortPrefix, prefix.Url);
                }
            }
        }

        private static class SparqlUtil
        {
            internal static readonly IList<SparqlPrefix> SparqlPrefixes = new List<SparqlPrefix>
            {
                // TODO: rename pid2 prefix to eco and merge pid and pid3 without # on pid (DB data transformation required)
                new SparqlPrefix("pid",  new Uri("https://pid.bayer.com/kos/19050#")),
                new SparqlPrefix("pid2", new Uri("http://pid.bayer.com/kos/19014/")),
                new SparqlPrefix("pid3", new Uri("https://pid.bayer.com/kos/19050/")),
                new SparqlPrefix("owl",  new Uri("http://www.w3.org/2002/07/owl#")),
                new SparqlPrefix("rdf",  new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#")),
                new SparqlPrefix("rdfs", new Uri("http://www.w3.org/2000/01/rdf-schema#")),
                new SparqlPrefix("skos", new Uri("http://www.w3.org/2004/02/skos/core#")),
                new SparqlPrefix("tosh", new Uri("http://topbraid.org/tosh#")),
                new SparqlPrefix("sh",   new Uri("http://www.w3.org/ns/shacl#")),
                new SparqlPrefix("xsd",  new Uri("http://www.w3.org/2001/XMLSchema#"))
            };
        }

        private class SparqlPrefix
        {
            public string ShortPrefix { get; set; }
            public Uri Url { get; set; }

            public SparqlPrefix(string shortPrefix, Uri url)
            {
                ShortPrefix = shortPrefix;
                Url = url;
            }
        }
    }
}
