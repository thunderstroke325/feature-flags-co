using FeatureFlags.APIs.Models;
using FeatureFlags.APIs.Services;
using Shouldly;
using Xunit;

namespace FeatureFlags.APIs.Tests
{
    public class EnvSecretTests
    {
        [Fact]
        public void Should_Generate_New_Env_Secret_Key()
        {
            var defaultKey = NewEnvSecret().New("default");
            var mobileKey = NewEnvSecret().New("mobile");

            defaultKey.ShouldNotBeNullOrWhiteSpace();
            mobileKey.ShouldNotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Should_Parse_Generated_Env_Secret_Key()
        {
            var envSecret = NewEnvSecret();
            var key = envSecret.New("whatever");

            var parsedEnvSecret = EnvironmentSecretV2.Parse(key);

            parsedEnvSecret.AccountId.ShouldBe(envSecret.AccountId);
            parsedEnvSecret.EnvId.ShouldBe(envSecret.EnvId);
            parsedEnvSecret.ProjectId.ShouldBe(envSecret.ProjectId);
        }

        [Fact]
        public void Should_Parse_Old_Env_Secret_Key()
        {
            var oldKey = FeatureFlagKeyExtension.GenerateEnvironmentKey(46, 57, 93);
            var newEnvSecret = EnvironmentSecretV2.Parse(oldKey);

            newEnvSecret.EnvId.ShouldBe(46);
            newEnvSecret.AccountId.ShouldBe(57);
            newEnvSecret.ProjectId.ShouldBe(93);
        }

        [Fact]
<<<<<<< HEAD
        public void Should_Throw_Exception_When_Parse_Invalid_Env_Secret_Key()
=======
        public void Should_Throw_Exception_When_Parse_Invalid_EnvSecret()
>>>>>>> bring newest code to migration (#196)
        {
            Should.Throw<InvalidEnvSecretException>(() => EnvironmentSecretV2.Parse(""));
            Should.Throw<InvalidEnvSecretException>(() => EnvironmentSecretV2.Parse(null));
            Should.Throw<InvalidEnvSecretException>(() => EnvironmentSecretV2.Parse("any other invalid secret"));
<<<<<<< HEAD
        }

        [Fact]
        public void Should_Try_Parse_Env_Secret_Key()
        {
            var parseInvalidKey = EnvironmentSecretV2.TryParse("invalid envSecret", out var invalidSecret);
            parseInvalidKey.ShouldBeFalse();
            invalidSecret.ShouldBeNull();
            
            var envSecret = NewEnvSecret();
            var validKey = envSecret.New("whatever");
            var parseValidKey = EnvironmentSecretV2.TryParse(validKey, out var validSecret);
            parseValidKey.ShouldBeTrue();
            validSecret.ShouldNotBeNull();
=======
>>>>>>> bring newest code to migration (#196)
        }

        private EnvironmentSecretV2 NewEnvSecret()
        {
            var envSecret = new EnvironmentSecretV2(46, 57, 93);
            return envSecret;
        }
    }
}