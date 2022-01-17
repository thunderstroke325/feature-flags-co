using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeatureFlags.APIs.Services.MongoDb
{
    public class MongoDbObjectIdEntity : IEntity<ObjectId>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }
    }
}