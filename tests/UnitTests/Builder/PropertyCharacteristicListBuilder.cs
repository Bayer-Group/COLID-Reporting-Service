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
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#2b3f0380-dd22-4666-a28b-7f1eeb82a5ff","471", "Data Services",200,271),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#bf2f8eeb-fdb9-4ee1-ad88-e8932fa8753c","309", "INDIGO",300,9),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#82fc2870-ca4e-407f-a197-bf3766ad785f","59", "DINOS",50,9),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050#3bb018e4-b006-4c9d-a85c-cd409fec89e5","36", "GeAR Metadata",30,6),
            };

            return this;
        }

        public PropertyCharacteristicListBuilder GenerateResourceTypeCharacteristics()
        {
            _characteristics = new List<PropertyCharacteristic>()
            {
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/GenericDataset","429", "Generic Dataset",420,29),
                new PropertyCharacteristic("https://pid.bayer.com/d188c668-b710-45b2-9631-faf29e85ac8d/RWD_Source","10", "RWD Source",5,5),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/MathematicalModel","5", "Mathematical Model",5,0),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/RDFDatasetWithInstances","4", "RDF Dataset with Instances",4,1),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Mapping","3", "Mapping",2,1)
            };

            return this;
        }

        public PropertyCharacteristicListBuilder GenerateInformationClassificationCharacteristics()
        {
            _characteristics = new List<PropertyCharacteristic>()
            {
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Restricted","585", "Restricted",500,85),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Internal","228", "Internal",200,28),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Open","44", "Open",40,4),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/Secret","17", "Secret",10,7)
            };

            return this;
        }

        public PropertyCharacteristicListBuilder GenerateLifecycleStatusCharacteristics()
        {
            _characteristics = new List<PropertyCharacteristic>()
            {
                new PropertyCharacteristic("https://pid/bayer/com/kos/19050/underDevelopment","13", "Under Development",10,3),
                new PropertyCharacteristic("https://pid.bayer.com/kos/19050/released","11", "Released",10,1),
            };

            return this;
        }
    }
}
