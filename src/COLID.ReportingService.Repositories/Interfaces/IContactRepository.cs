using System;
using System.Collections.Generic;
using System.Text;
using COLID.ReportingService.Common.DataModels;

namespace COLID.ReportingService.Repositories.Interfaces
{
    public interface IContactRepository
    {
        /// <summary>
        /// Returns a list containing all contacts referenced in the database
        /// </summary>
        /// <returns>A list of contacts ids</returns>
        IEnumerable<string> GetContacts();

        /// <summary>
        /// Returns a list of entries in which the user is referenced. 
        /// Instead of the contact of the consumer group the id is stored in the object. 
        /// In the contact service the contact including email is added to the consumer group.
        /// </summary>
        /// <param name="userEmailAddress">The email address of the user to search for.</param>
        /// <param name="resourceTypes">A list of resource types of the referenced entries</param>
        ///<param name="contactTypes">A list of contact types</param>
        /// <returns>List of all entries with their contacts.</returns>
        IEnumerable<ColidEntryContactsCto> GetContactReferencedEntries(string userEmailAddress, IEnumerable<string> resourceTypes, IEnumerable<string> contactTypes);
    }
}
