using System;
using System.Collections.Generic;
using System.Text;

namespace COLID.ReportingService.Common.Constants
{
    public static class CacheKeys
    {
        public static class Statistics
        {
            public const string TotalNumberOfResources = "totalnumberofresources";
            public const string TotalNumberOfProperties = "totalnumberofproperties";
            public const string TotalPropertyValuesOfAllResources = "totalpropertyvaluesofallresources";
            public const string TotalNumberOfResourcesByPredicate = "totalnumberofresourcesbypredicate";
            public const string TotalNumberOfVersionsOfResources = "totalnumberofversionsofresources";
            public const string TotalNumberOfPropertyUsageByGroupOfResource = "totalnumberofpropertyusagebygroupofresource";
            public const string TotalResourceTypeCharacteristics = "totalresourcetypecharacteristics";
            public const string TotalConsumerGroupCharacteristics = "totalconsumergroupcharacteristics";
            public const string TotalInformationClassificationCharacteristics = "totalinformationclassificationcharacteristics";
            public const string TotalLifecycleStatusCharacteristics = "totallifecyclestatuscharacteristics";
            public const string TotalUsageCountByPredicate = "totalusagecountbypredicate";
        }

        public static class CallingClass
        {
            public const string ResourceStatisticsService = "resourcestatisticsservice";
        }

        public static class Resource  
        {
            public const string COLIDResources = "https://pid.bayer.com/kos/19050/444556";
            public const string CoreEntities = "https://pid.bayer.com/kos/19050/444558";
        }
    }
}
