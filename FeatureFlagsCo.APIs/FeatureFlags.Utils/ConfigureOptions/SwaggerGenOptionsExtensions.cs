using FeatureFlags.Utils.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FeatureFlags.Utils.ConfigureOptions
{
    public static class SwaggerGenOptionsExtensions
    {
        public static void AddJwtAuth(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                              "Example Input: **Bearer the-token-string**",
                Name = "Authorization",
                Scheme = "Bearer",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        }

        public static void AddPublicApiAuth(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition(RequestAuthKeys.EnvSecret, new OpenApiSecurityScheme
            {
                Name = RequestAuthKeys.EnvSecret,
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "envSecret Authorization for **Public Api**, fill your envSecret below."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = RequestAuthKeys.EnvSecret
                        }
                    },
                    new string[] { }
                }
            });
        }
    }
}