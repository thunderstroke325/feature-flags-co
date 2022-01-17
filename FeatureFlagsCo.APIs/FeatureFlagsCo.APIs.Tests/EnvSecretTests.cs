using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class EnvSecretTests
    {
        [Fact]
        public void Should_Generate_New_Key()
        {
            var secret = NewEnvSecret().New("default");
            var mobileSecret = NewEnvSecret().New("mobile");

            secret.ShouldNotBeNullOrWhiteSpace();
            mobileSecret.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_Parse_Generated_Key()
        {
            var envSecret = NewEnvSecret();

            var parsedEnvSecret = EnvironmentSecretV2.Parse(envSecret.New("whatever"));

            parsedEnvSecret.AccountId.ShouldBe(envSecret.AccountId);
            parsedEnvSecret.EnvId.ShouldBe(envSecret.EnvId);
            parsedEnvSecret.ProjectId.ShouldBe(envSecret.ProjectId);
        }

        [Fact]
        public void Should_Parse_Old_Key()
        {
            var oldEnvSecret = FeatureFlagKeyExtension.GenerateEnvironmentKey(1, 1, 1);
            var newEnvSecret = EnvironmentSecretV2.Parse(oldEnvSecret);
            
            newEnvSecret.EnvId.ShouldBe(1);
            newEnvSecret.AccountId.ShouldBe(1);
            newEnvSecret.ProjectId.ShouldBe(1);
        }

        private EnvironmentSecretV2 NewEnvSecret()
        {
            var envSecret = new EnvironmentSecretV2(1, 2, 3);
            return envSecret;
        }
    }
}