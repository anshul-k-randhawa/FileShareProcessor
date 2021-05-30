using System.Text.RegularExpressions;

namespace FileShareProcessor.Helpers
{
    public static class StringHelper
    {
        public static bool IsPatternMatch(this string line, string pattern)
        {
            return Regex.IsMatch(line, WildCardToRegular(pattern));
        }

        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
