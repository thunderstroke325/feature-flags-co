using FeatureFlags.APIs.Repositories;
using System;
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
        public void Test1()
        {

        }

        [Fact]
        public void IfHashedValueIsFixed()
        {
            var _variationService = new VariationService(null, null);

            var result = _variationService.IfBelongRolloutPercentage("hu-beau@outlook.com", new double[] { 0.683, 0.685 });

            Assert.True(result);
        }

        [Fact]
        public void IfBelongRolloutPercentage()
        {
            var _variationService = new VariationService(null, null);
            
            for (int k = 0; k < 10; k++)
            {
                string returnValue = "";
                returnValue += $"   ##Rround {k}## ";
                int trueCount = 0;
                for (int i = 0; i < 100; i++)
                {
                    if (_variationService.IfBelongRolloutPercentage(Guid.NewGuid().ToString(), new double[] { 0.0, 0.333 }))
                        trueCount++;
                }
                returnValue += $" 100 sample (GUID): {trueCount};";
                trueCount = 0;
                for (int i = 0; i < 100; i++)
                {
                    if (_variationService.IfBelongRolloutPercentage($"hu-beau{1000 * k + i}@outlook.com", new double[] { 0.0, 0.333 }))
                        trueCount++;
                }
                returnValue += $" 100 sample (小变化邮箱): {trueCount};";


                trueCount = 0;
                for (int i = 0; i < 1000; i++)
                {
                    if (_variationService.IfBelongRolloutPercentage(Guid.NewGuid().ToString(), new double[] { 0.0, 0.333 }))
                        trueCount++;
                }
                returnValue += $" 1000 sample (GUID): {trueCount};";
                trueCount = 0;
                for (int i = 0; i < 1000; i++)
                {
                    if (_variationService.IfBelongRolloutPercentage($"hu-beau{10000 * k + i}@outlook.com", new double[] { 0.0, 0.333 }))
                        trueCount++;
                }
                returnValue += $" 1000 sample (小变化邮箱): {trueCount};";

                trueCount = 0;
                for (int i = 0; i < 10000; i++)
                {
                    if (_variationService.IfBelongRolloutPercentage(Guid.NewGuid().ToString(), new double[] { 0.0, 0.333 }))
                        trueCount++;
                }
                returnValue += $" 10000 sample: {trueCount};";
                trueCount = 0;
                for (int i = 0; i < 10000; i++)
                {
                    if (_variationService.IfBelongRolloutPercentage($"hu-beau{100000 * k + i}@outlook.com", new double[] { 0.0, 0.333 }))
                        trueCount++;
                }
                returnValue += $" 10000 sample (小变化邮箱): {trueCount};";


                _testOutputHelper.WriteLine(returnValue);
            }
        }
    }
}
