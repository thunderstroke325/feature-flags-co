using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace FeatureFlagsCo.MQ.ElasticSearch
{
    public static class ElasticSearchServiceProviderExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["MySettings:ElasticSearchHost"].TrimEnd('/');
          
            var connectionSettings = new ConnectionSettings(new Uri(connectionString));
            
            services.AddSingleton(new ElasticClient(connectionSettings));
            services.AddScoped<ElasticSearchService>();
        }
    }
}