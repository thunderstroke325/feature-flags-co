using System;
using MongoDB.Bson.Serialization.Attributes;

namespace FeatureFlags.APIs.Services.MongoDb
{
    /// <summary>
    /// collection id counter
    /// </summary>
    public class CollectionIdCounter
    {
        /// <summary>
        /// the id of the counter, we should use the name of the collection in most scenarios 
        /// </summary>
        [BsonId]
        public string Id { get; set; }

        /// <summary>
        /// the current sequence for the collection, that means the next id of the collection would be Sequence + 1
        /// </summary>
        public int Sequence { get; set; }

        public CollectionIdCounter(string id, int sequence = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("collection id counter's id cannot be null or empty");
            }
            Id = id;
            
            Sequence = sequence;
        }
    }
}