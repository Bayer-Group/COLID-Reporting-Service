using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using COLID.ReportingService.Common.DataModels;

namespace COLID.ReportingService.Services.Interfaces
{
    public interface IContactService
    {
        /// <summary>
        /// Returns a list containing all contacts referenced in the database
        /// </summary>
        /// <returns>A list of contacts ids</returns>
        IEnumerable<string> GetContacts();

        /// <summary>
        /// Returns a list of entries in which the user is referenced. 
        /// </summary>
        /// <param name="userEmailAddress">The email address of the user to search for.</param>
        /// <returns>List of all entries with their contact contacts.</returns>
        Task<IEnumerable<ColidEntryContactsCto>> GetContactReferencedEntries(string userEmailAddress);
    }
}
