using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.Utils.DependencyInjectionExtensions
{
    public interface INamedTransientDependency : ISingletonDependency, INamedDependency
    {
    }
}