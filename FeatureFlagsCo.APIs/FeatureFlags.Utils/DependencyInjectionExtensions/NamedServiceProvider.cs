using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Utils.DependencyInjectionExtensions
{
    public class NamedServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public NamedServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TINamedDependency GetService<TINamedDependency>(string name)
            where TINamedDependency : INamedDependency
        {
            var services = _serviceProvider.GetServices<TINamedDependency>();

            if (string.IsNullOrWhiteSpace(name))
            {
                return services.Last();
            }

            var namedDependency = services.FirstOrDefault(x => x.Name == name);
            if (namedDependency == null)
            {
                throw new InvalidOperationException($"No service of type {typeof(TINamedDependency).Name} with name {name} was found.");
            }
            
            return namedDependency;
        }
    }
}