using System;
using System.Collections.Generic;
using System.Text;

namespace COLID.ReportingService.Common.DataModels
{
    public class PropertyStatistics
    {
        public string Name { get; set; }

        public int Increment { get; set; }

        public IList<PropertyStatisticItem> Counts { get; set; }

        public PropertyStatistics()
        {
            Counts = new List<PropertyStatisticItem>();
        }

        public PropertyStatistics(string name, int increment, IList<PropertyStatisticItem> counts)
        {
            Name = name;
            Increment = increment;
            Counts = counts;
        }
    }
}
