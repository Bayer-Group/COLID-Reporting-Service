using System;
using System.Net.Mime;
using COLID.ReportingService.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace COLID.ReportingService.WebApi.Controllers
{
    /// <summary>
    /// API endpoint for statistics.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/statistics")]
    [Produces(MediaTypeNames.Application.Json)]
    public class StatisticsController : ControllerBase
    {
        private readonly IResourceStatisticsService _resourceStatisticsService;

        /// <summary>
        /// API endpoint for statistics information.
        /// </summary>
        /// <param name="resourceStatisticsService">The service for status information</param>
        public StatisticsController(IResourceStatisticsService resourceStatisticsService)
        {
            _resourceStatisticsService = resourceStatisticsService;
        }

        /// <summary>
        /// Returns the number of resources.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Returns the number of resources</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/total")]
        public IActionResult GetTotalNumberOfResources()
        {
            return Ok(_resourceStatisticsService.GetTotalNumberOfResources());
        }

        /// <summary>
        /// Returns the number of selected possibilities of one property
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="property"></param>
        /// <response code="200">Returns the number of selected possibilities of one property</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/controlledvocabularyselection")]
        public IActionResult GetNumberOfControlledVocabularySelection([FromQuery] Uri property)
        {
            return Ok(_resourceStatisticsService.GetNumberOfControlledVocabularySelection(property));
        }

        /// <summary>
        /// Returns how often a property was used.
        /// </summary>
        /// <response code="200">Returns how often a property was used</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/numberofproperties")]
        public IActionResult GetNumberOfProperties()
        {
            return Ok(_resourceStatisticsService.GetNumberOfProperties());
        }

        /// <summary>
        /// Returns the number of resources in relation to property length
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="property"></param>
        /// <param name="increment"></param>
        /// <response code="200">number of resources in relation to property length</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/numberofresourcesinrelationtopropertylength")]
        public IActionResult GetNumberOfResourcesInRelationToPropertyLength([FromQuery] Uri property, [FromQuery] int increment)
        {
            return Ok(_resourceStatisticsService.GetNumberOfResourcesInRelationToNumberOfPropertyWords(property, increment));
        }

        /// <summary>
        /// Returns the number of versions of one pid entry
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="increment"></param>
        /// <response code="200">The number of versions of one pid entry in triplestore</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/numberofversionsofresources")]
        public IActionResult GetNumberOfVersionsOfResources([FromQuery] int increment)
        {
            return Ok(_resourceStatisticsService.GetNumberOfVersionsOfResources(increment));
        }

        /// <summary>
        /// Returns the number of property usage of group properties
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="group"></param>
        /// <response code="200">Returns the number of property usage of group properties</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/numberofpropertyusagebygroup")]
        public IActionResult GetNumberOfPropertyUsageByGroupOfResource([FromQuery] Uri group)
        {
            return Ok(_resourceStatisticsService.GetNumberOfPropertyUsageByGroupOfResource(group));
        }

        /// <summary>
        /// Returns the number of different expressions of colid types
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Return list of expression counts</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/characteristics/type")]
        public IActionResult GetResourceTypeCharacteristics()
        {
            return Ok(_resourceStatisticsService.GetResourceTypeCharacteristics());
        }

        /// <summary>
        /// Returns the number of different expressions of used consumer groups
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Return list of expression counts</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/characteristics/consumergroup")]
        public IActionResult GetConsumerGroupCharacteristics()
        {
            return Ok(_resourceStatisticsService.GetConsumerGroupCharacteristics());
        }

        /// <summary>
        /// Returns the number of different expressions of used information classification
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Return list of expression counts</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/characteristics/informationclassification")]
        public IActionResult GetInformationClassificationCharacteristics()
        {
            return Ok(_resourceStatisticsService.GetInformationClassificationCharacteristics());
        }

        /// <summary>
        /// Returns the number of different expressions of resource lifecycle status
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Return list of expression counts</response>
        /// <response code="400">If an invalid uri is given</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("resource/characteristics/lifecyclestatus")]
        public IActionResult GetLifecycleStatusCharacteristics()
        {
            return Ok(_resourceStatisticsService.GetLifecycleStatusCharacteristics());
        }
    }
}
