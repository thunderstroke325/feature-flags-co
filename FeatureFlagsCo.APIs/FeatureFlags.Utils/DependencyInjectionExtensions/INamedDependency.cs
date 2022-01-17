using FeatureFlags.Utils.ConventionalDependencyInjection;

namespace FeatureFlags.Utils.DependencyInjectionExtensions
{
    public interface INamedDependency : IDependency
    {
        string Name { get; }
    }
}