using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FeatureFlags.APIs.Services.MongoDb
{
    public class MongoDbIntIdEntity : IEntity<int>
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId DocumentId { get; set; }

        public int Id { get; protected set; }

        public void SetId(int id)
        {
            if (id == 0)
            {
                throw new ArgumentException("id cannot be 0");
            }

            Id = id;
        }
    }
}