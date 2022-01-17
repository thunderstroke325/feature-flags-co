using System;

namespace FeatureFlags.APIs.Services.MongoDb
{
    public class MongoDbRepositoryException : Exception
    {
        public MongoDbRepositoryException(string message) : base(message)
        {
        }
    }
}