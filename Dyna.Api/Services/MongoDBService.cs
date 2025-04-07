using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Dyna.Api.Services
{
    public interface IMongoDBService
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
        Task<List<Dictionary<string, object>>> FindDocumentsAsync(string collectionName, FilterDefinition<BsonDocument> filter);
        Task<Dictionary<string, object>?> FindDocumentByIdAsync(string collectionName, string id);
        Task<BsonDocument?> GetSampleDocument(string collectionName);
    }

    public class MongoDBService : IMongoDBService
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDBService> _logger;

        public MongoDBService(IConfiguration configuration, ILogger<MongoDBService> logger)
        {
            var connectionString = configuration.GetConnectionString("local");
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("dyna_content");
            _logger = logger;
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        public async Task<List<Dictionary<string, object>>> FindDocumentsAsync(string collectionName, FilterDefinition<BsonDocument> filter)
        {
            var collection = GetCollection<BsonDocument>(collectionName);
            
            // Log the filter definition
            _logger.LogInformation($"MongoDB Query Filter: {filter.ToString()}");
            
            var documents = await collection.Find(filter).ToListAsync();
            _logger.LogInformation($"Found {documents.Count} documents");
            
            if (documents.Count == 0)
            {
                // Get a sample document to verify structure
                var sample = await GetSampleDocument(collectionName);
                if (sample != null)
                {
                    _logger.LogInformation($"Sample document structure: {sample.ToJson()}");
                }
            }
            
            return documents.Select(ConvertBsonToJson).ToList();
        }

        public async Task<BsonDocument?> GetSampleDocument(string collectionName)
        {
            var collection = GetCollection<BsonDocument>(collectionName);
            return await collection.Find(new BsonDocument())
                                 .Limit(1)
                                 .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<string, object>?> FindDocumentByIdAsync(string collectionName, string id)
        {
            var collection = GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            var document = await collection.Find(filter).FirstOrDefaultAsync();
            return document != null ? ConvertBsonToJson(document) : null;
        }

        private Dictionary<string, object> ConvertBsonToJson(BsonDocument document)
        {
            var result = new Dictionary<string, object>();
            foreach (var element in document.Elements)
            {
                result[element.Name] = ConvertBsonValueToJson(element.Value);
            }
            return result;
        }

        private object ConvertBsonValueToJson(BsonValue value)
        {
            switch (value.BsonType)
            {
                case BsonType.ObjectId:
                    return value.AsObjectId.ToString();
                case BsonType.DateTime:
                    return value.ToUniversalTime();
                case BsonType.String:
                    return value.AsString;
                case BsonType.Int32:
                    return value.AsInt32;
                case BsonType.Int64:
                    return value.AsInt64;
                case BsonType.Double:
                    return value.AsDouble;
                case BsonType.Boolean:
                    return value.AsBoolean;
                case BsonType.Null:
                    return null;
                case BsonType.Document:
                    return ConvertBsonToJson(value.AsBsonDocument);
                case BsonType.Array:
                    return value.AsBsonArray.Select(ConvertBsonValueToJson).ToList();
                default:
                    return value.ToString();
            }
        }
    }
} 