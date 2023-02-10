using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

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
            private static readonly string _basePath = Path.GetFullPath("appsettings.json");
            private static readonly string _filePath = _basePath.Substring(0, _basePath.Length - 16);
            private static IConfigurationRoot _configuration = new ConfigurationBuilder()
                         .SetBasePath(_filePath)
                        .AddJsonFile("appsettings.json")
                        .Build();
            public static readonly string _serviceUrl = _configuration.GetValue<string>("ServiceUrl");
            public static readonly string COLIDResources = _serviceUrl + "kos/19050/444556";
            public static readonly string CoreEntities = _serviceUrl + "kos/19050/444558";
        }
    }
}
