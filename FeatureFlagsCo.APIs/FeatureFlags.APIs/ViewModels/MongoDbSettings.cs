using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels
{

    public interface IMongoDbSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
    public class MongoDbSettings : IMongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
