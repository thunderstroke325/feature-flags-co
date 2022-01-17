using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.Utils.DependencyInjectionExtensions
{
    public interface INamedSingletonDependency : ISingletonDependency, INamedDependency
    {
    }
}