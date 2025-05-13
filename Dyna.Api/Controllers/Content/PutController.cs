using Microsoft.AspNetCore.Mvc;
using Dyna.Api.Services; // Your namespace
using MongoDB.Bson;
using MongoDB.Driver;
using System;                   // Added for DateTime
using System.Threading.Tasks;   // Added for Task
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335; // Added for ILogger (assuming it's used elsewhere or you want to add logging)

namespace Dyna.Api.Controllers.Content // Your namespace
{
    [ApiController]
    [Route("Content/Put")]
    public class PutContentController : ControllerBase
    {
        // --- Assuming your constructor and other fields are here ---
        private readonly ILogger<PutContentController> _logger;
        // private readonly string _ContentFilePath; // This wasn't used in the methods shown
        private readonly IMongoDBService _mongoDBService;

        public PutContentController(
            IWebHostEnvironment env,
            IFileService fileService,
            IMongoDBService mongoDBService,
            ILogger<PutContentController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }
        // --- End Constructor/Fields ---

        private async Task<IActionResult> PutEntities(Dictionary<string, object> arguments)
        {
            // Handle for one or many objects received to create one or many entities

            string? id = null;
            string? collection = null;
            DateTime? createdFrom = null;
            DateTime? createdTo = null;
            DateTime? updatedFrom = null;
            DateTime? updatedTo = null;
            DateTime? activeFrom = null;
            DateTime? activeTo = null;
            string? campaignId = null;
            if (arguments != null)
            {
                if (arguments.ContainsKey("collection") && arguments["collection"] is String currentCollection)
                {
                    collection = currentCollection;
                }
                if (arguments.ContainsKey("id") && arguments["id"] is String currentId)
                {
                    id = currentId;
                }
                if (arguments.ContainsKey("campaignId") && arguments["campaignId"] is String currentCampaignId)
                {
                    campaignId = currentCampaignId;
                }
                if (arguments.ContainsKey("createdFrom") && arguments["createdFrom"] is DateTime currentCreatedFrom)
                {
                    createdFrom = currentCreatedFrom;
                }
                if (arguments.ContainsKey("createdTo") && arguments["createdTo"] is DateTime currentCreatedTo)
                {
                    createdTo = currentCreatedTo;
                }
                if (arguments.ContainsKey("updatedFrom") && arguments["updatedFrom"] is DateTime currentUpdatedFrom)
                {
                    updatedFrom = currentUpdatedFrom;
                }
                if (arguments.ContainsKey("updatedTo") && arguments["updatedTo"] is DateTime currentUpdatedTo)
                {
                    updatedTo = currentUpdatedTo;
                }
                if (arguments.ContainsKey("activeFrom") && arguments["activeFrom"] is DateTime currentActiveFrom)
                {
                    activeFrom = currentActiveFrom;
                }
                if (arguments.ContainsKey("activeTo") && arguments["activeTo"] is DateTime currentActiveTo)
                {
                    activeTo = currentActiveTo;
                }
            }
            try
            {

                // Execute query
                if (collection != null)
                {
                    return Ok("Modified");
                }
                else
                {
                    _logger.LogWarning("No collection provided: {Collection}", collection);
                    return BadRequest("No collection selected");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving entities");
            }
        }

        [HttpPut("Assets/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> PutAssets([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","assets"},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await PutEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving assets");
            }
        }

        [HttpPut("Campaigns/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> PutCampaigns([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] DateTime? activeFrom = null, [FromQuery] DateTime? activeTo = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "campaigns_creatives" : "campaigns");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","campaigns"},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                    { "activeFrom", activeFrom},
                    { "activeTo", activeTo},
                };
                return await PutEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving campaigns");
            }
        }

        [HttpPut("Components/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> PutComponents([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "components_hierarchy" : "components");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection", collection},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await PutEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving components");
            }
        }

        [HttpPut("Creatives/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> PutCreatives([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? creativeId = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "creatives_elements" : "creatives");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection",collection},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await PutEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving creatives");
            }
        }

        [HttpPut("Elements/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> PutElements([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? campaignId = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "all_elements_with_components" : "elements");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection",collection},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                    { "campaignId", campaignId},
                    { "nested", nested}
                };
                return await PutEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving elements");
            }
        }

        [HttpPut("Formats/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> PutFormats([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","formats"},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await PutEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving format");
            }
        }

        [HttpPut("Samples/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> PutSamples([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","samples"},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await PutEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving samples");
            }
        }
    } // End Class
} // End Namespace