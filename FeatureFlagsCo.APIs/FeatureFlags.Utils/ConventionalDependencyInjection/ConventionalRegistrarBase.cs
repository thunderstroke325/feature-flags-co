using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FeatureFlags.Utils.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FeatureFlags.Utils.ConventionalDependencyInjection
{
    public abstract class ConventionalRegistrarBase : IConventionalRegistrar
    {
        public virtual void AddAssembly(IServiceCollection services, Assembly assembly)
        {
            var types = AssemblyHelper
                .GetAllTypes(assembly)
                .Where(
                    type => type != null &&
                            type.IsClass &&
                            type.IsAssignableTo(typeof(IDependency)) &&
                            !type.IsAbstract
                ).ToArray();

            AddTypes(services, types);
        }

        public virtual void AddTypes(IServiceCollection services, params Type[] types)
        {
            foreach (var type in types)
            {
                AddType(services, type);
            }
        }

        public abstract void AddType(IServiceCollection services, Type type);

        protected virtual ServiceLifetime? GetLifeTimeOrNull(Type type)
        {
            return GetServiceLifetimeFromClassHierarchy(type) ?? GetDefaultLifeTimeOrNull(type);
        }

        protected virtual ServiceLifetime? GetServiceLifetimeFromClassHierarchy(Type type)
        {
            if (typeof(ITransientDependency).GetTypeInfo().IsAssignableFrom(type))
            {
                return ServiceLifetime.Transient;
            }

            if (typeof(ISingletonDependency).GetTypeInfo().IsAssignableFrom(type))
            {
                return ServiceLifetime.Singleton;
            }

            if (typeof(IScopedDependency).GetTypeInfo().IsAssignableFrom(type))
            {
                return ServiceLifetime.Scoped;
            }

            return null;
        }

        protected virtual ServiceLifetime? GetDefaultLifeTimeOrNull(Type type)
        {
            return null;
        }

        protected virtual List<Type> GetDefaultExposedServiceTypes(Type type)
        {
            var serviceTypes = new List<Type>();

            foreach (var interfaceType in type.GetTypeInfo().GetInterfaces())
            {
                var interfaceName = interfaceType.Name;

                if (interfaceName.StartsWith("I"))
                {
                    interfaceName = interfaceName.Substring(1, interfaceName.Length - 1);
                }

                if (type.Name.Contains(interfaceName))
                {
                    serviceTypes.Add(interfaceType);
                }
            }

            return serviceTypes;
        }

        protected virtual void RegisterService(
            IServiceCollection services,
            Type serviceType,
            Type implementationType, 
            ServiceLifetime lifeTime)
        {
            var serviceDescriptor = ServiceDescriptor.Describe(serviceType, implementationType, lifeTime);
            
            services.Add(serviceDescriptor);
        }
    }
}