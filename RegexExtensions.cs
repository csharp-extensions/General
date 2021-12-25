using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CsharpExtensions
{
    public static class RegexExtensions
    {
        public static string? GetGroupValueOrNull(this Regex regex, string source, int groupIndex = 1, bool checkEmpty = true) => GetGroupValue(regex, source, groupIndex, false, checkEmpty);
        public static string GetGroupValueOrThrow(this Regex regex, string source, int groupIndex = 1, bool checkEmpty = true) => GetGroupValue(regex, source, groupIndex, true, checkEmpty)!;
        public static string? GetGroupValue(this Regex regex, string source, int groupIndex = 1, bool throwOnNull = false, bool checkEmpty = true)
        {
            if (regex != null && !string.IsNullOrEmpty(source))
            {
                var match = regex.Match(source);
                if (match.Success)
                {
                    var group = match.Groups[groupIndex];
                    if (group.Success)
                    {
                        if (!checkEmpty || !string.IsNullOrEmpty(group.Value))
                        {
                            return group.Value;
                        }
                    }
                }
            }
            if (throwOnNull)
            {
                throw new Exception($"failed to get the value of capturing group {groupIndex}, regex pattern: {regex}, source: {source}");
            }
            return null;
        }

        public static List<string> GetGroupValuesOrNull(this Regex regex, string source, int groupIndex = 1)
        {
            var list = new List<string>();

            var matches = regex.Matches(source);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var group = match.Groups[groupIndex];
                    if (group.Success && !string.IsNullOrEmpty(group.Value))
                    {
                        list.Add(group.Value);
                    }
                }
            }
            return list;
        }
    }
}
