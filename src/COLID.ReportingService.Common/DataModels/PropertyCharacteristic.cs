using System;
using System.Collections.Generic;
using System.Text;

namespace COLID.ReportingService.Common.DataModels
{
    public class PropertyCharacteristic
    {
        public string Key { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }

        public PropertyCharacteristic() { }

        public PropertyCharacteristic(string key, string count, string name) { }
    }
}
