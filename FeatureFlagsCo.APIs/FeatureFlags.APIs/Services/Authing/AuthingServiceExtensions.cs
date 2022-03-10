using Authing.ApiClient.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.APIs.Services.Authing
{
    public static class AuthingServiceExtensions
    {
        public static void AddAuthingClient(this IServiceCollection services, IConfiguration config)
        {
            var appId = config["Authing:AppId"];

            var client = new AuthenticationClient(options =>
            {
                options.AppId = appId;
            });

            services.AddSingleton(client);
        }
    }
}