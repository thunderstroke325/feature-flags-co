using System.Collections.Generic;
using FeatureFlags.APIs.Models;
using MongoDB.Bson.Serialization;

namespace FeatureFlags.APIs.Services.MongoDb
{
    public static class MongoClassMapping
    {
        public static void Register()
        {
            // register custom mapping here
            // see http://mongodb.github.io/mongo-csharp-driver/2.14/reference/bson/mapping/#mapping-classes

            BsonClassMap.RegisterClassMap<EnvironmentV2>(env =>
            {
                env.AutoMap();
                env.MapMember(x => x.Settings).SetDefaultValue(new List<EnvironmentSettingV2>());
            });
        }
    }
}