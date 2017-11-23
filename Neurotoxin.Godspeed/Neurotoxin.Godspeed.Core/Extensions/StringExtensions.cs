using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Hash(this string s)
        {
            var shA1Managed = new SHA1Managed();
            return shA1Managed.ComputeHash(Encoding.UTF8.GetBytes(s)).ToHex();
        }

        public static string GetParentPath(this string path)
        {
            path = "/" + path.TrimEnd('/', '\\');
            var r = new Regex(@"^(.*[\\/]).*$");
            return r.Replace(path, "$1").Substring(1);
        }

        public static string SubstringBefore(this string haystack, char needle)
        {
            return SubstringBefore(haystack, new string(needle, 1));
        }

        public static string SubstringBefore(this string haystack, string needle)
        {
            var i = haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase);
            return i > -1 ? haystack.Substring(0, i) : null;
        }

        public static string SubstringAfter(this string haystack, char needle)
        {
            return SubstringAfter(haystack, new string(needle, 1));
        }

        public static string SubstringAfter(this string haystack, string needle)
        {
            var i = haystack.IndexOf(needle, StringComparison.InvariantCultureIgnoreCase);
            return i > -1 ? haystack.Substring(i + needle.Length) : null;
        }
    }
}
