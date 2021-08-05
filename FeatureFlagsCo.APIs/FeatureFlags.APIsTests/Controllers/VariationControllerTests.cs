using Microsoft.VisualStudio.TestTools.UnitTesting;
using FeatureFlags.APIs.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using FeatureFlags.APIs.Repositories;

namespace FeatureFlags.APIs.Controllers.Tests
{
    [TestClass()]
    public class VariationControllerTests
    {
        [TestMethod()]
        public void RolloutPercentage()
        {
            var _variationService = new VariationService(null, null, null);

            var result = _variationService.IfBelongRolloutPercentage("hu-beau@outlook.com", new double[] { 0.683, 0.685 });

            Assert.IsTrue(result);
        }


        [TestMethod()]
        public void IfBelongRolloutPercentage()
        {
            var _variationService = new VariationService(null, null, null);

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

                Console.WriteLine(returnValue);

            }
        }
    }
}