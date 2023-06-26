using System.Net.Mime;
using System.Threading.Tasks;
using COLID.ReportingService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace COLID.ReportingService.WebApi.Controllers
{
    /// <summary>
    /// API endpoint for contacts.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/contact")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        /// <summary>
        /// API endpoint for contact related information.
        /// </summary>
        /// <param name="contactService">The service for contact related information</param>
        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        /// <summary>
        /// Returns a list containing all contacts referenced in the database.
        /// </summary>
        /// <response code="200">Returns a list of contacts ids</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet]
        public IActionResult GetContacts()
        {
            return Ok(_contactService.GetContacts());
        }

        /// <summary>
        /// Returns a list containing all contacts referenced in the database.
        /// </summary>
        /// <response code="200">Returns a list of contacts ids</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("{userEmailAddress}/colidEntries")]
        public async Task<IActionResult> GetContactReferencedEntries(string userEmailAddress)
        {
            var referencedEntries = await _contactService.GetContactReferencedEntries(userEmailAddress);
            return Ok(referencedEntries);
        }
    }
}
