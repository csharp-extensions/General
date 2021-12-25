using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace CSharpExtensions.OpenSource
{
    public static class CookieExtensions
    {
        private static Regex rxCookieParts = new Regex(@"(?<name>.*?)\=(?<value>.*?)\;|(?<name>\bsecure\b|\bhttponly\b)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static Regex rxRemoveCommaFromDate = new Regex(@"\bexpires\b\=.*?(\;|$)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);
        private static string RemoveComma(Match match) => match.Value.Replace(',', ' ');
        public static List<Cookie> GetCookiesFromHeaders(this Dictionary<string, string>? headers, string url)
        {
            var lst = new List<Cookie>();
            var cookiesFromHeaderValues = headers?.Keys.Where(x => x.ToLower().Contains("cookie")).Select(x => headers![x]).Where(x=> !string.IsNullOrEmpty(x?.Trim()));
            foreach (var cookieHeaderVal in cookiesFromHeaderValues.RemoveNulls())
            {
                lst.AddRange(cookieHeaderVal.GetCookiesFromHeader(url));
            }
            return lst;
        }
        public static List<Cookie> GetCookiesFromHeader(this string cookieHeader, string url)
        {
            var domain = new Uri(url).Host;
            var cookies = new List<Cookie>();
            var rawcookieString = rxRemoveCommaFromDate.Replace(cookieHeader, new MatchEvaluator(RemoveComma));
            var rawCookies = rawcookieString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            rawCookies = rawCookies.Length == 0 ? new[] { rawcookieString } : rawCookies;
            foreach (var rawCookie in rawCookies)
            {
                var maches = rxCookieParts.Matches(!rawCookie.EndsWith(";") ? rawCookie + ";" : rawCookie);
                var cookie = new Cookie(maches[0].Groups["name"].Value.Trim(), maches[0].Groups["value"].Value.Trim());
                for (int i = 1; i < maches.Count; i++)
                {
                    switch (maches[i].Groups["name"].Value.ToLower().Trim())
                    {
                        case "domain":
                            cookie.Domain = maches[i].Groups["value"].Value;
                            break;
                        case "expires":
                            if (DateTime.TryParse(maches[i].Groups["value"].Value, out var dt))
                            {
                                cookie.Expires = dt;
                            }
                            else
                            {
                                cookie.Expires = DateTime.Now.AddDays(2);
                            }
                            break;
                        case "path":
                            cookie.Path = maches[i].Groups["value"].Value;
                            break;
                        case "secure":
                            cookie.Secure = true;
                            break;
                        case "httponly":
                            cookie.HttpOnly = true;
                            break;
                    }
                }
                cookie.Domain = string.IsNullOrEmpty(cookie.Domain) ? domain : cookie.Domain;
                cookies.Add(cookie);
            }
            return cookies;
        }
    }
}
