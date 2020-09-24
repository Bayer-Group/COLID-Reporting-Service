using System;
using System.Collections.Generic;
using System.Text;
using COLID.ReportingService.Common.DataModels;

namespace COLID.ReportingService.UnitTests.Builder
{
    public class PropertyCharacteristicListBuilder
    {
        private IList<PropertyCharacteristic> _characteristics;

        public PropertyCharacteristicListBuilder()
        {
            _characteristics = new List<PropertyCharacteristic>();
        }

        public IList<PropertyCharacteristic> Build()
        {
            return _characteristics;
        }

        public PropertyCharacteristicListBuilder GenerateConsumerGroupCharacteristics()
        {
            _characteristics = new List<PropertyCharacteristic>()
            {
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#2b3f0380-dd22-4666-a28b-7f1eeb82a5ff","471", "Data Services"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#bf2f8eeb-fdb9-4ee1-ad88-e8932fa8753c","309", "INDIGO"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#82fc2870-ca4e-407f-a197-bf3766ad785f","59", "DINOS"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#3bb018e4-b006-4c9d-a85c-cd409fec89e5","36", "GeAR Metadata"),
            };

            return this;
        }

        public PropertyCharacteristicListBuilder GenerateResourceTypeCharacteristics()
        {
            _characteristics = new List<PropertyCharacteristic>()
            {
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/GenericDataset","429", "Generic Dataset"),
                new PropertyCharacteristic("https://pid.bayer.com/d188c668-b710-45b2-9631-faf29e85ac8d/RWD_Source","10", "RWD Source"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/MathematicalModel","5", "Mathematical Model"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/RDFDatasetWithInstances","4", "RDF Dataset with Instances"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Mapping","3", "Mapping")
            };

            return this;
        }

        public PropertyCharacteristicListBuilder GenerateInformationClassificationCharacteristics()
        {
            _characteristics = new List<PropertyCharacteristic>()
            {
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Restricted","585", "Restricted"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Internal","228", "Internal"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Open","44", "Open"),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Secret","17", "Secret")
            };

            return this;
        }
    }
}
