using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FeatureFlags.Utils.ExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FeatureFlags API",
                    Description = "backend api for feature flags",
                });

                options.AddJwtAuth();
                options.AddPublicApiAuth();
            });
        }

        public static SwaggerGenOptions AddJwtAuth(this SwaggerGenOptions options)
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

            return options;
        }

        public static SwaggerGenOptions AddPublicApiAuth(this SwaggerGenOptions options)
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

            return options;
        }
    }
}