using System;

namespace FeatureFlagsCo.MQ.ElasticSearch
{
    public class ElasticSearchException : Exception
    {
        public ElasticSearchException()
        {
        }

        public ElasticSearchException(string message)
            : base(message)
        {
        }

        public ElasticSearchException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}