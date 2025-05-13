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
    [Route("Content/Delete")]
    public class DeleteContentController : ControllerBase
    {
        // --- Assuming your constructor and other fields are here ---
        private readonly ILogger<DeleteContentController> _logger;
        // private readonly string _ContentFilePath; // This wasn't used in the methods shown
        private readonly IMongoDBService _mongoDBService;

        public DeleteContentController(
            IWebHostEnvironment env,
            IFileService fileService,
            IMongoDBService mongoDBService,
            ILogger<DeleteContentController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }
        // --- End Constructor/Fields ---

        private async Task<IActionResult> DeleteEntities(Dictionary<string, object> arguments)
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
                    return Ok("Deleted");
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

        [HttpDelete("Assets/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> DeleteAssets([FromRoute] string? id = null, [FromBody] DateTime? payload = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","assets"},
                    { "id",id}
                };
                return await DeleteEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving assets");
            }
        }

        [HttpDelete("Campaigns/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> DeleteCampaigns([FromRoute] string? id = null, [FromBody] DateTime? payload = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","campaigns"},
                    { "id",id}
                };
                return await DeleteEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving campaigns");
            }
        }

        [HttpDelete("Components/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> DeleteComponents([FromRoute] string? id = null, [FromBody] DateTime? payload = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection", "components"},
                    { "id",id}
                };
                return await DeleteEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving components");
            }
        }

        [HttpDelete("Creatives/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> DeleteCreatives([FromRoute] string? id = null, [FromBody] DateTime? payload = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","creatives"},
                    { "id",id}
                };
                return await DeleteEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving creatives");
            }
        }

        [HttpDelete("Elements/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> DeleteElements([FromRoute] string? id = null, [FromBody] DateTime? payload = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","elements"},
                    { "id",id}
                };
                return await DeleteEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving elements");
            }
        }

        [HttpDelete("Formats/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> DeleteFormats([FromRoute] string? id = null, [FromBody] DateTime? payload = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","formats"},
                    { "id",id}
                };
                return await DeleteEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving format");
            }
        }

        [HttpDelete("Samples/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> DeleteSamples([FromRoute] string? id = null, [FromBody] DateTime? payload = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","samples"},
                    { "id",id}
                };
                return await DeleteEntities(arguments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving samples");
            }
        }
    } // End Class
} // End Namespace