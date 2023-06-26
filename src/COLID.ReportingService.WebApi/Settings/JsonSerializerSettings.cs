using System.Collections.Generic;
using COLID.Cache.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace COLID.ReportingService.WebApi.Settings
{
    public static class JsonSerializerSettings
    {
        public static CachingJsonSerializerSettings GetSerializerSettings()
        {
            var serializerSettings = new CachingJsonSerializerSettings
            {
                Converters = new List<JsonConverter>(),
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };

            return serializerSettings;
        }
    }
}
