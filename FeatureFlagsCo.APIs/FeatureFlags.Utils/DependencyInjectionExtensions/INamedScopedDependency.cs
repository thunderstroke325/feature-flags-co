using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.Utils.DependencyInjectionExtensions
{
    public interface INamedScopedDependency : IScopedDependency, INamedDependency
    {
    }
}