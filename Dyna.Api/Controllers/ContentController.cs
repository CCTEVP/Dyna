using Microsoft.AspNetCore.Mvc;
using Dyna.Api.Services; // Your namespace
using MongoDB.Bson;
using MongoDB.Driver;
using System;                   // Added for DateTime
using System.Threading.Tasks;   // Added for Task
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis; // Added for ILogger (assuming it's used elsewhere or you want to add logging)

namespace Dyna.Api.Controllers // Your namespace
{
    [ApiController]
    [Route("[controller]")]
    public class ContentController : ControllerBase
    {
        // --- Assuming your constructor and other fields are here ---
        private readonly ILogger<ContentController> _logger;
        // private readonly string _ContentFilePath; // This wasn't used in the methods shown
        private readonly IMongoDBService _mongoDBService;

        public ContentController(
            IWebHostEnvironment env,
            IFileService fileService,
            IMongoDBService mongoDBService,
            ILogger<ContentController> logger)
        {
            _mongoDBService = mongoDBService;
            _logger = logger;
        }
        // --- End Constructor/Fields ---

        // --- Other methods like GetCampaigns, etc. ---
        [HttpGet("Campaigns/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetCampaigns([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] DateTime? activeFrom = null, [FromQuery] DateTime? activeTo = null, [FromQuery] Boolean? active = null)
        {
            // ... Your existing implementation ...
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _))
                    {
                        var campaign = await _mongoDBService.FindDocumentByIdAsync("campaigns", id);
                        if (campaign == null)
                        {
                            return NotFound($"Campaign with ID {id} not found");
                        }
                        return Ok(new[] { campaign });
                    }
                    else
                    {
                        return BadRequest("Invalid campaign ID format");
                    }
                }
                // --- Handle filtered list request ---
                var filterBuilder = Builders<BsonDocument>.Filter; // Easier to reuse builder
                var filter = filterBuilder.Empty;

                // Optional: Adjust date field names if they are different in MongoDB (e.g., "created")
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
                    filter &= filterBuilder.Gte("ends", activeFrom.Value);
                }

                if (activeTo.HasValue)
                {
                    filter &= filterBuilder.Lte("starts", activeTo.Value);
                }

                if (active.HasValue && active.Value)
                {
                    DateTime currentDateTime = DateTime.UtcNow;
                    filter &= filterBuilder.Lte("starts", currentDateTime) & filterBuilder.Gte("ends", currentDateTime);
                    _logger.LogWarning("Current time is " + currentDateTime);
                }

                var campaigns = await _mongoDBService.FindDocumentsAsync("campaigns", filter);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns");
                return StatusCode(500, "An error occurred while retrieving campaigns");
            }
        }

        // --- CORRECTED GetCreatives Method ---
        [HttpGet("Creatives/{id?}")]
        [ApiExplorerSettings(IgnoreApi = false)]
        public async Task<IActionResult> GetCreatives([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? campaignId = null, [FromQuery] Boolean? nested = null)
        {
            try
            {
                string currentCollection = (nested.HasValue && nested.Value) ? "creatives_elements" : "creatives";

                // --- Handle specific ID request ---
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _)) // Don't need the parsed Id here currently
                    {
                        var creative = await _mongoDBService.FindDocumentByIdAsync(currentCollection, id);
                        if (creative == null)
                        {
                            return NotFound($"Creative with ID {id} not found");
                        }
                        return Ok(creative); // Return as array for consistency
                    }
                    else
                    {
                        _logger.LogWarning("Invalid creative ID format provided: {CreativeId}", id);
                        return BadRequest("Invalid creative ID format");
                    }
                }

                // --- Handle filtered list request ---
                var filterBuilder = Builders<BsonDocument>.Filter; // Easier to reuse builder
                var filter = filterBuilder.Empty;

                // Optional: Adjust date field names if they are different in MongoDB (e.g., "created")
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

                // Apply campaign filter if provided
                if (!string.IsNullOrEmpty(campaignId))
                {
                    // *** FIX: Parse the campaignId string into an ObjectId ***
                    if (ObjectId.TryParse(campaignId, out var campaignObjectId)) // <-- Store parsed ObjectId
                    {
                        _logger.LogWarning("Parsed campaignObjectId: {CampaignObjectId}", campaignObjectId);
                        // *** FIX: Use the parsed campaignObjectId in AnyEq ***
                        filter &= filterBuilder.AnyEq("parent", campaignObjectId); // <-- Use the ObjectId here
                    }
                    else
                    {
                        _logger.LogWarning("Invalid Campaign ID format provided: {CampaignId}", campaignId);
                        return BadRequest("Invalid Campaign ID format");
                    }
                }

                // Execute query
                var creatives = await _mongoDBService.FindDocumentsAsync(currentCollection, filter);
                _logger.LogInformation("Executing creatives filter: {Filter}", filter.ToString());
                _logger.LogInformation("Found {CreativeCount} creatives.", creatives.Count);
                return Ok(new []{creatives});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving creatives. ID: {CreativeId}, CampaignID: {CampaignId}", id, campaignId);
                return StatusCode(500, "An error occurred while retrieving creatives");
            }
        }

        [HttpGet("Elements/{id?}")]
        public async Task<IActionResult> GetElements([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? type = null, [FromQuery] Boolean? nested = null)
        {
            try
            {
                string currentCollection = (nested.HasValue && nested.Value) ? "elements_contents" : "elements";
                
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _))
                    {
                        var element = await _mongoDBService.FindDocumentByIdAsync(currentCollection, id);
                        if (element == null) { return NotFound($"Element with ID {id} not found"); }
                        return Ok(element);
                    }
                    else { return BadRequest("Invalid element ID format"); }
                }
                var filter = Builders<BsonDocument>.Filter.Empty;
                if (createdFrom.HasValue) { filter &= Builders<BsonDocument>.Filter.Gte("createdFrom", createdFrom.Value); }
                if (createdTo.HasValue) { filter &= Builders<BsonDocument>.Filter.Lte("createdTo", createdTo.Value); }
                if (!string.IsNullOrEmpty(type)) { filter &= Builders<BsonDocument>.Filter.Eq("type", type); }
                var elements = await _mongoDBService.FindDocumentsAsync(currentCollection, filter);
                return Ok(new []{elements});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving elements");
                return StatusCode(500, "An error occurred while retrieving elements");
            }
        }

        [HttpGet("Formats/{id?}")]
        public async Task<IActionResult> GetFormats([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _))
                    {
                        var format = await _mongoDBService.FindDocumentByIdAsync("formats", id);
                        if (format == null) { return NotFound($"Format with ID {id} not found"); }
                        return Ok(format);
                    }
                    else { return BadRequest("Invalid format ID format"); }
                }
                var filter = Builders<BsonDocument>.Filter.Empty;
                if (createdFrom.HasValue) { filter &= Builders<BsonDocument>.Filter.Gte("createdFrom", createdFrom.Value); }
                if (createdTo.HasValue) { filter &= Builders<BsonDocument>.Filter.Lte("createdTo", createdTo.Value); }
                var formats = await _mongoDBService.FindDocumentsAsync("formats", filter);
                return Ok(new []{formats});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving formats");
                return StatusCode(500, "An error occurred while retrieving formats");
            }
        }

        [HttpGet("Assets/{id?}")]
        public async Task<IActionResult> GetAssets([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? type = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _))
                    {
                        var asset = await _mongoDBService.FindDocumentByIdAsync("assets", id);
                        if (asset == null) { return NotFound($"Asset with ID {id} not found"); }
                        return Ok( asset );
                    }
                    else { return BadRequest("Invalid asset ID format"); }
                }
                var filter = Builders<BsonDocument>.Filter.Empty;
                if (createdFrom.HasValue) { filter &= Builders<BsonDocument>.Filter.Gte("createdFrom", createdFrom.Value); }
                if (createdTo.HasValue) { filter &= Builders<BsonDocument>.Filter.Lte("createdTo", createdTo.Value); }
                if (!string.IsNullOrEmpty(type)) { filter &= Builders<BsonDocument>.Filter.Eq("type", type); }
                var assets = await _mongoDBService.FindDocumentsAsync("assets", filter);
                return Ok(new []{assets});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assets");
                return StatusCode(500, "An error occurred while retrieving assets");
            }
        }

        [HttpGet("Templates/{id?}")]
        public async Task<IActionResult> GetTemplates([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _))
                    {
                        var template = await _mongoDBService.FindDocumentByIdAsync("templates", id);
                        if (template == null) { return NotFound($"Template with ID {id} not found"); }
                        return Ok(template);
                    }
                    else { return BadRequest("Invalid template ID format"); }
                }
                var filter = Builders<BsonDocument>.Filter.Empty;
                if (createdFrom.HasValue) { filter &= Builders<BsonDocument>.Filter.Gte("createdFrom", createdFrom.Value); }
                if (createdTo.HasValue) { filter &= Builders<BsonDocument>.Filter.Lte("createdTo", createdTo.Value); }
                var templates = await _mongoDBService.FindDocumentsAsync("templates", filter);
                return Ok(new []{templates});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving templates");
                return StatusCode(500, "An error occurred while retrieving templates");
            }
        }

        [HttpGet("BaseComponents/{id?}")]
        public async Task<IActionResult> GetComponents([FromRoute] string? id = null, [FromQuery] DateTime? createdFrom = null, [FromQuery] DateTime? createdTo = null, [FromQuery] DateTime? updatedFrom = null, [FromQuery] DateTime? updatedTo = null, [FromQuery] string? type = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    if (ObjectId.TryParse(id, out _))
                    {
                        var component = await _mongoDBService.FindDocumentByIdAsync("components", id);
                        if (component == null) { return NotFound($"Component with ID {id} not found"); }
                        return Ok(component);
                    }
                    else { return BadRequest("Invalid component ID format"); }
                }
                var filter = Builders<BsonDocument>.Filter.Empty;
                if (createdFrom.HasValue) { filter &= Builders<BsonDocument>.Filter.Gte("createdFrom", createdFrom.Value); }
                if (createdTo.HasValue) { filter &= Builders<BsonDocument>.Filter.Lte("createdTo", createdTo.Value); }
                if (!string.IsNullOrEmpty(type)) { filter &= Builders<BsonDocument>.Filter.Eq("type", type); }
                var components = await _mongoDBService.FindDocumentsAsync("components", filter);
                return Ok(new []{components});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving components");
                return StatusCode(500, "An error occurred while retrieving components");
            }
        }

    } // End Class
} // End Namespace