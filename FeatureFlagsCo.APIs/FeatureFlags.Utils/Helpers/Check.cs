using System;
using JetBrains.Annotations;

namespace FeatureFlags.Utils.Helpers
{
    public static class Check
    {
        public static T NotNull<T>(
            T value, 
            [InvokerParameterName] [NotNull] string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        public static string NotNullOrWhiteSpace(
            string value,
            [InvokerParameterName] [NotNull] string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{parameterName} can not be null, empty or white space!", parameterName);
            }
            
            return value;
        }
    }
}