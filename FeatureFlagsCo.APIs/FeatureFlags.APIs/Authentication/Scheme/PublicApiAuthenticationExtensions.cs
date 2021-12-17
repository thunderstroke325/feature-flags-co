using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.APIs.Authentication.Scheme
{
    public static class PublicApiAuthenticationExtensions
    {
        public static void AddPublicApiAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<PublicApiAuthenticationOptions, PublicApiAuthenticationHandler>(
                    PublicApiAuthenticationConstants.Scheme, options => { });
        }
    }
}