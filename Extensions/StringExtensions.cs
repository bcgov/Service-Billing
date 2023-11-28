using System;
using System.Linq;

namespace Service_Billing.Extensions
{
    public static class StringExtensions
    {
        public static bool IsContainedIgnoringOrderAndCase(this string value1, string value2)
        {
            var words1 = value1.NormalizeAndSplit();
            var words2 = value2.NormalizeAndSplit();

            return words1.All(word1 => words2.Any(word2 => string.Equals(word1, word2, StringComparison.OrdinalIgnoreCase)));
        }

        private static string[] NormalizeAndSplit(this string value)
        {
            return value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(word => new string(word.Where(char.IsLetterOrDigit).ToArray()))
                        .ToArray();
        }
    }

}
