using MongoDB.Bson;
using Dyna.Api.Models;

namespace Dyna.Api.Utilities
{
    public class DataBuilder
    {
        public BsonDocument document = new BsonDocument();
        public DataBuilder(BsonDocument document) { 
            this.document = document;
        }
       
    }
}
