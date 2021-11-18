using System;
using System.Linq;
using System.Text;
using FeatureFlags.APIs.Services;
using Xunit;
using Xunit.Abstractions;

namespace FeatureFlagsCo.APIs.Tests
{
    public class RolloutPercentageTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RolloutPercentageTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void The_Hashed_Value_Should_Be_Fixed()
        {
            var result = VariationSplittingAlgorithm.IfKeyBelongsPercentage("hu-beau@outlook.com", new double[] { 0.683, 0.685 });

            Assert.True(result);
        }

        [Fact]
        public void Should_Belongs_To_Percentage_Evenly()
        {
            foreach (var round in Enumerable.Range(0, 10))
            {
                var result = new StringBuilder($"   ##Round {round}## ");

                const string guidKeyType = "GUID";
                const string emailKeyType = "EMAIL";
                
                result.Append(RunSample(round, 100, guidKeyType));
                result.Append(RunSample(round, 100, emailKeyType));
                
                result.Append(RunSample(round, 1000, guidKeyType));
                result.Append(RunSample(round, 1000, emailKeyType));
                
                result.Append(RunSample(round, 10000, guidKeyType));
                result.Append(RunSample(round, 10000, emailKeyType));
                
                _testOutputHelper.WriteLine(result.ToString());
            }
        }

        private static string RunSample(int round, int count, string keyType)
        {
            var belongsToRangeCount = 0;
            var percentageRange = new []{ 0.0, 0.333 };
            
            for (var i = 0; i < count; i++)
            {
                var key = keyType == "GUID" ? Guid.NewGuid().ToString() : $"hu-beau{1000 * round + i}@outlook.com";
                if (VariationSplittingAlgorithm.IfKeyBelongsPercentage(key, percentageRange))
                {
                    belongsToRangeCount++;
                }
            }

            return $" {count} sample ({(keyType == "GUID" ? "GUID" : "Ð¡±ä»¯ÓÊÏä")}): {belongsToRangeCount};";
        }
    }
}
