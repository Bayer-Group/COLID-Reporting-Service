using System;
using System.Collections.Generic;
using System.Text;

namespace COLID.ReportingService.Common.DataModels
{
    public class PropertyMetadata
    {
        public string GroupType { get; set; }
        public string ResourceType { get; set; }
        public string ShaclConstraint { get; set; }
        public string PropertyUri { get; set; }
        public string PropertyName { get; set; }
        public bool IsMandatory { get; set; }        
    }
}
