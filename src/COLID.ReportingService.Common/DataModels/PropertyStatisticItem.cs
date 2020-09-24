using System;
using System.Collections.Generic;
using System.Text;

namespace COLID.ReportingService.Common.DataModels
{
    public class PropertyStatisticItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Property { get; set; }
        public string Total { get; set; }

        public PropertyStatisticItem() { }

        public PropertyStatisticItem(string key, string value, string property, string total = null)
        {
            Key = key;
            Value = value;
            Property = property;
            Total = total;
        }
    }
}
