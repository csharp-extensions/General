using Newtonsoft.Json.Serialization;

namespace CsharpExtensions
{
    public static class StringExtensions
    {
        public static string CleanupNewline(this string text) => text.Replace("\n", "")
                                                  .Replace("\r", "")
                                                  .Replace("\r\n", "")
                                                  .Replace("\\\n", "")
                                                  .Replace("\\\r", "")
                                                  .Replace("\\\r\\\n", "")
                                                  .Replace("\\n", "")
                                                  .Replace("\\r", "")
                                                  .Replace("\\r\\n", "");

        public static string? ToCamelCase(this string? str) => str is null
            ? null
            : new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() }.GetResolvedPropertyName(str);

        public static string? ToSnakeCase(this string? str) => str is null
            ? null
            : new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }.GetResolvedPropertyName(str);

        public static string? DetectAsNull(this string? str, params string[] vals)
        {
            var res = str?.CleanupNewline().Trim();
            if (string.IsNullOrEmpty(res)) { return null; }
            foreach (var val in vals)
            {
                if (res == val) { return null; }
            }
            return str;
        }

        /// <summary>
        /// Method that limits the length of text to a defined length.
        /// </summary>
        /// <param name="source">The source text.</param>
        /// <param name="maxLength">The maximum limit of the string to return.</param>
        public static string LimitLength(this string source, int maxLength)
        {
            if (source.Length <= maxLength) { return source; }
            return source.Substring(0, maxLength);
        }

        public static string TrimStart(this string target, string? trimString)
        {
            if (string.IsNullOrEmpty(trimString)) { return target; }
            var result = target;
            while (result.StartsWith(trimString)) { result = result.Substring(trimString.Length); }
            return result;
        }

        public static string TrimEnd(this string target, string? trimString)
        {
            if (string.IsNullOrEmpty(trimString)) { return target; }
            var result = target;
            while (result.EndsWith(trimString)) { result = result.Substring(0, result.Length - trimString.Length); }
            return result;
        }
        public static string Trim(this string target, string? trimString) => target.TrimStart(trimString).TrimEnd(trimString);
        // check if string empty
        public static bool IsEmpty(this string? str) => str == null || string.IsNullOrEmpty(str);
    }
}
