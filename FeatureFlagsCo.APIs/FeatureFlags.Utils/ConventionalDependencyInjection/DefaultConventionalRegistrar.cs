using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Utils.ConventionalDependencyInjection
{
    public class DefaultConventionalRegistrar : ConventionalRegistrarBase
    {
        public override void AddType(IServiceCollection services, Type type)
        {
            var lifeTime = GetLifeTimeOrNull(type);
            if (lifeTime == null)
            {
                return;
            }

            var exposedServices = GetDefaultExposedServiceTypes(type);
            foreach (var exposedServiceType in exposedServices)
            {
                RegisterService(services, exposedServiceType, type, lifeTime.Value);
            }

            // if there is no explicit exposed services, add the type itself as an exposed service
            if (!exposedServices.Any())
            {
                RegisterService(services, type, type, lifeTime.Value);
            }
        }
    }
}