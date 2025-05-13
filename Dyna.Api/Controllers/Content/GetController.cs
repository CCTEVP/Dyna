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
    [Route("Content/Get")]
    public class GetContentController : ControllerBase
    {
        // --- Assuming your constructor and other fields are here ---
        private readonly ILogger<GetContentController> _logger;
        // private readonly string _ContentFilePath; // This wasn't used in the methods shown
        private readonly IMongoDBService _mongoDBService;

        public GetContentController(
            IWebHostEnvironment env,
            IFileService fileService,
            IMongoDBService mongoDBService,
            ILogger<GetContentController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }
        // --- End Constructor/Fields ---

        private async Task<IActionResult> GetEntities(Dictionary<string, object> arguments)
        {
            string? collection = null;
            string? id = null;
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

                // --- Handle specific ID request ---
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _))
                    {
                        if (collection != null)
                        {
                            var asset = await _mongoDBService.FindDocumentByIdAsync(collection, id);
                            if (asset == null)
                            {
                                return NotFound($"Entity with ID {id} not found");
                            }
                            return Ok(asset);
                        }
                        else
                        {
                            _logger.LogWarning("No collection provided: {Collection}", collection);
                            return BadRequest("No collection selected");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid entity ID format provided: {EntityId}", id);
                        return BadRequest("Invalid entity ID format");
                    }
                }
                // --- Handle filtered list request ---
                var filterBuilder = Builders<BsonDocument>.Filter;
                var filter = filterBuilder.Empty;

                if (createdFrom.HasValue)
                {
                    filter &= filterBuilder.Gte("created", createdFrom.Value);
                }
                if (createdTo.HasValue)
                {
                    filter &= filterBuilder.Lte("created", createdTo.Value);
                }
                if (updatedFrom.HasValue)
                {
                    filter &= filterBuilder.Gte("updated", updatedFrom.Value);
                }
                if (updatedTo.HasValue)
                {
                    filter &= filterBuilder.Lte("updated", updatedTo.Value);
                }
                if (activeFrom.HasValue)
                {
                    filter &= filterBuilder.Gte("starts", activeFrom.Value);
                }
                if (activeTo.HasValue)
                {
                    filter &= filterBuilder.Lte("ends", activeTo.Value);
                }

                // Execute query
                if (collection != null)
                {
                    var entities = await _mongoDBService.FindDocumentsAsync(collection, filter);
                    _logger.LogInformation("Executing entities filter: {Filter}", filter.ToString());
                    _logger.LogInformation("Found {EntitiesCount} entities.", entities.Count);
                    return Ok(entities);
                }
                else
                {
                    _logger.LogWarning("No collection provided: {Collection}", collection);
                    return BadRequest("No collection selected");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entities. ID: {AssetId}", id);
                return StatusCode(500, "An error occurred while retrieving entities");
            }
        }

        [HttpGet("Assets/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetAssets([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","assets"},
                    { "id",id},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await GetEntities(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assets. ID: {AssetId}", id);
                return StatusCode(500, "An error occurred while retrieving assets");
            }
        }

        [HttpGet("Campaigns/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetCampaigns([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] DateTime? activeFrom = null, [FromQuery] DateTime? activeTo = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "campaigns_creatives" : "campaigns");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","campaigns"},
                    { "id",id},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                    { "activeFrom", activeFrom},
                    { "activeTo", activeTo},
                };
                return await GetEntities(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns. ID: {CampaignId}", id);
                return StatusCode(500, "An error occurred while retrieving campaigns");
            }
        }

        [HttpGet("Components/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetComponents([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "components_hierarchy" : "components");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection", collection},
                    { "id",id},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await GetEntities(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving components. ID: {ComponentId}", id);
                return StatusCode(500, "An error occurred while retrieving components");
            }
        }

        [HttpGet("Creatives/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetCreatives([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? creativeId = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "creatives_elements" : "creatives");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection",collection},
                    { "id",id},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await GetEntities(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving creatives. ID: {CreativeId}", id);
                return StatusCode(500, "An error occurred while retrieving creatives");
            }
        }

        [HttpGet("Elements/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetElements([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? campaignId = null, [FromQuery] Boolean? nested = null)
        {
            string collection = ((nested.HasValue && nested.Value) ? "elements_binding" : "elements");
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection",collection},
                    { "id",id},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                    { "campaignId", campaignId},
                    { "nested", nested}
                };
                return await GetEntities(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving elements. ID: {ElementId}", id);
                return StatusCode(500, "An error occurred while retrieving elements");
            }
        }

        [HttpGet("Formats/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetFormats([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","formats"},
                    { "id",id},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await GetEntities(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving format. ID: {FormatId}", id);
                return StatusCode(500, "An error occurred while retrieving format");
            }
        }

        [HttpGet("Samples/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetSamples([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                Dictionary<string, object> arguments = new Dictionary<string, object>() {
                    { "collection","samples"},
                    { "id",id},
                    { "createdFrom", createdFrom},
                    { "createdTo", createdTo},
                    { "updatedFrom", updatedFrom},
                    { "updatedTo", updatedTo},
                };
                return await GetEntities(arguments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving samples. ID: {SampleId}", id);
                return StatusCode(500, "An error occurred while retrieving samples");
            }
        }
    } // End Class
} // End Namespace