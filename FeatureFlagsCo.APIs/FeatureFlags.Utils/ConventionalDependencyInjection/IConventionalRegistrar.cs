using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Utils.ConventionalDependencyInjection
{
    public interface IConventionalRegistrar
    {
        void AddAssembly(IServiceCollection services, Assembly assembly);

        void AddTypes(IServiceCollection services, params Type[] types);

        void AddType(IServiceCollection services, Type type);
    }
}