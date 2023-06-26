using System.Collections.Generic;
using COLID.Graph.TripleStore.DataModels.ConsumerGroups;
using COLID.Graph.TripleStore.Extensions;

namespace UnitTests.Builder
{
    public class ConsumerGroupBuilder : AbstractEntityBuilder<ConsumerGroup>
    {
        private ConsumerGroup _cg = new ConsumerGroup();

        public override ConsumerGroup Build()
        {
            _cg.Properties = _prop;
            return _cg;
        }

        public ConsumerGroupResultDTO BuildResultDTO()
        {
            return new ConsumerGroupResultDTO()
            {
                Id = _cg.Id,
                Name = _prop.GetValueOrNull(COLID.Graph.Metadata.Constants.RDFS.Label, true),
                Properties = _prop
            };
        }

        public ConsumerGroupBuilder GenerateSampleData()
        {
            WithType();
            WithLifecycleStatus(COLID.Graph.Metadata.Constants.ConsumerGroup.LifecycleStatus.Active);
            WithLabel("GeAR Metadata");
            WithContactPerson("superadmin@bayer.com");
            WithAdRole("PID.Group05Data.ReadWrite");
            WithId("https://pid.bayer.com/kos/19050#3bb018e4-b006-4c9d-a85c-cd409fec89e5");
            WithPidUriTemplate("https://pid.bayer.com/kos/19050#fd1d2aa4-ee2a-427e-a891-ee03c1549eac");

            return this;
        }

        public IEnumerable<ConsumerGroupResultDTO> GenerateSampleDataList()
        {
            var cg1 = new ConsumerGroupBuilder()
                .GenerateSampleData()
                .WithLabel("GeAR Metadata")
                .WithContactPerson("superadmin@bayer.com")
                .WithAdRole("PID.Group05Data.ReadWrite")
                .WithId("https://pid.bayer.com/kos/19050#3bb018e4-b006-4c9d-a85c-cd409fec89e5")
                .WithPidUriTemplate("https://pid.bayer.com/kos/19050#fd1d2aa4-ee2a-427e-a891-ee03c1549eac")
                .BuildResultDTO();

            var cg2 = new ConsumerGroupBuilder()
                .GenerateSampleData()
                .WithLabel("INDIGO")
                .WithContactPerson("anotheruser@bayer.com")
                .WithAdRole("PID.Group01Data.ReadWrite")
                .WithId("https://pid.bayer.com/kos/19050#bf2f8eeb-fdb9-4ee1-ad88-e8932fa8753c")
                .WithPidUriTemplate("https://pid.bayer.com/kos/19050#5a9bc613-f948-4dd3-8cd7-9a4465319d24")
                .BuildResultDTO();

            return new List<ConsumerGroupResultDTO>() { cg1, cg2 };
        }



        public ConsumerGroupBuilder WithId(string id)
        {
            _cg.Id = id;
            return this;
        }

        public ConsumerGroupBuilder WithType()
        {
            CreateOrOverwriteProperty(COLID.Graph.Metadata.Constants.RDF.Type, COLID.Graph.Metadata.Constants.ConsumerGroup.Type);
            return this;
        }

        public ConsumerGroupBuilder WithLifecycleStatus(string lifecycleStatus)
        {
            CreateOrOverwriteProperty(COLID.Graph.Metadata.Constants.ConsumerGroup.HasLifecycleStatus, lifecycleStatus);
            return this;
        }

        public ConsumerGroupBuilder WithContactPerson(string contactPerson)
        {
            CreateOrOverwriteProperty(COLID.Graph.Metadata.Constants.ConsumerGroup.HasContactPerson, contactPerson);
            return this;
        }

        public ConsumerGroupBuilder WithLabel(string label)
        {
            CreateOrOverwriteProperty(COLID.Graph.Metadata.Constants.RDFS.Label, label);
            return this;
        }

        public ConsumerGroupBuilder WithAdRole(string adRole)
        {
            CreateOrOverwriteProperty(COLID.Graph.Metadata.Constants.ConsumerGroup.AdRole, adRole);
            return this;
        }

        public ConsumerGroupBuilder WithPidUriTemplate(string template)
        {
            CreateOrOverwriteProperty(COLID.Graph.Metadata.Constants.ConsumerGroup.HasPidUriTemplate, template);
            return this;
        }
    }
}
