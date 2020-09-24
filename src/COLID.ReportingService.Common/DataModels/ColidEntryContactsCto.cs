using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace COLID.ReportingService.Common.DataModels
{
    public class ColidEntryContactsCto
    {
        /// <summary>
        /// PidUri of colid entry
        /// </summary>
        public Uri PidUri { get; set; }

        /// <summary>
        /// Label of colid entry
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// List of all contacts with her roles in the entry
        /// </summary>
        public IEnumerable<ContactCto> Contacts { get; set; }

        /// <summary>
        /// Contact of the consumer group assigned to the entry
        /// </summary>
        public ContactCto ConsumerGroupContact { get; set; }

        /// <summary>
        /// Consumer group assigned to the entry
        /// </summary>
        [JsonIgnore]
        public Uri ConsumerGroup { get; set; }
    }
}
