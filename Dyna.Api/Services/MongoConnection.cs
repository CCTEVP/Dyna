using MongoDB.Bson;
using MongoDB.Driver;
using Dyna.Api.Models;
using Dyna.Api.Utilities;

namespace Dyna.Api.Services
{
    public class MongoConnection
    {
        public string country { get; set; }
        public BsonDocument currentDocument { get; set; }

        public MongoConnection(string country)
        {
            this.country = country;
        }
    }
}
