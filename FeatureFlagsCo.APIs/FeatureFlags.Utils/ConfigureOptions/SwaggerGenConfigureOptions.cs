using FeatureFlags.Utils.ExtensionMethods;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FeatureFlags.Utils.ConfigureOptions
{
    public class SwaggerGenConfigureOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public SwaggerGenConfigureOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            // add swagger document for every API version discovered
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
            }
            
            // add swagger authentication
            options.AddJwtAuth();
            options.AddPublicApiAuth();

            OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
            {
                var info = new OpenApiInfo
                {
                    Title = "FeatureFlags API",
                    Version = description.ApiVersion.ToString(),
                    Description = "backend api for feature flags",
                };

                if (description.IsDeprecated)
                {
                    info.Description += "<span style=\"color:red\"> This API version has been deprecated.</span>";
                }

                return info;
            }
        }
    }
}