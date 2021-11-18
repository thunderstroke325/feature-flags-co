using System;
using System.Security.Cryptography;
using System.Text;

namespace FeatureFlags.APIs.Services
{
    public class VariationSplittingAlgorithm
    {
        /// <summary>
        /// 对于一个给定的键值, 判断其是否属于某个概率范围内
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="percentageRange">概率范围, 比如 [0, 0.75]</param>
        /// <returns></returns>
        public static bool IfKeyBelongsPercentage(string key, double[] percentageRange)
        {
            var hashedKey = new MD5CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes(key));
            var magicNumber = BitConverter.ToInt32(hashedKey, 0);
            var percentage = Math.Abs((double) magicNumber / int.MinValue);
            
            return percentage >= percentageRange[0] && percentage <= percentageRange[1];
        }
    }
}