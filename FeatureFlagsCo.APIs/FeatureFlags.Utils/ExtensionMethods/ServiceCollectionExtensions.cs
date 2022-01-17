using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;
using FeatureFlags.Utils.ConfigureOptions;
using FeatureFlags.Utils.ConventionalDependencyInjection;
using FeatureFlags.Utils.DependencyInjectionExtensions;
using FeatureFlags.Utils.SwaggerOperationFilters;
using Microsoft.Extensions.Options;

namespace FeatureFlags.Utils.ExtensionMethods
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApiVersion(this IServiceCollection services)
        {
            // add api versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = ApiVersion.Default;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerGenConfigureOptions>();
            services.AddSwaggerGen(options =>
            {
                // add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();
            });
        }

        public static void AddAssembly(this IServiceCollection services, Assembly assembly)
        {
            var registrar = new DefaultConventionalRegistrar();
            
            registrar.AddAssembly(services, assembly);
        }

        public static void AddNamedServiceProvider(this IServiceCollection services)
        {
            services.AddSingleton<NamedServiceProvider>();
        }
    }
}