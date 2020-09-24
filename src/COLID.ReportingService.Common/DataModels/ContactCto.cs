using System;
using System.Collections.Generic;
using System.Text;

namespace COLID.ReportingService.Common.DataModels
{
    public class ContactCto
    {
        /// <summary>
        /// The email address of the contact
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// The contact type as an URI from graph.
        /// </summary>
        public Uri TypeUri { get; set; }

        /// <summary>
        /// The type name of the contact in the entry. (e.g. Data steward, author, etc)
        /// </summary>
        public string TypeLabel { get; set; }

        /// <summary>
        /// Determines if user is technical contact
        /// </summary>
        public bool IsTechnicalContact { get; set; }
    }
}
