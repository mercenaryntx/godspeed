using System;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Neurotoxin.Godspeed.Core.Net
{
    /// <summary>
    /// Extension methods related to FTP tasks
    /// </summary>
    public static class FtpExtensions
    {
        /// <summary>
        /// Converts the specified path into a valid FTP file system path
        /// </summary>
        /// <param name="path">The file system path</param>
        /// <returns>A path formatted for FTP</returns>
        public static string GetFtpPath(this string path)
        {
            if (String.IsNullOrEmpty(path))
                return "./";

            path = Regex.Replace(path.Replace('\\', '/'), "[/]+", "/").TrimEnd('/');
            if (path.Length == 0)
                path = "/";

            return path;
        }

        /// <summary>
        /// Creates a valid FTP path by appending the specified segments to this string
        /// </summary>
        /// <param name="path">This string</param>
        /// <param name="segments">The path segments to append</param>
        /// <returns>A valid FTP path</returns>
        public static string GetFtpPath(this string path, params string[] segments)
        {
            if (String.IsNullOrEmpty(path))
                path = "./";

            foreach (string part in segments)
            {
                if (part != null)
                {
                    if (path.Length > 0 && !path.EndsWith("/"))
                        path += "/";
                    path += Regex.Replace(part.Replace('\\', '/'), "[/]+", "/").TrimEnd('/');
                }
            }

            path = Regex.Replace(path.Replace('\\', '/'), "[/]+", "/").TrimEnd('/');
            if (path.Length == 0)
                path = "/";

            return path;
        }

        /// <summary>
        /// Gets the directory name of a path formatted for a FTP server
        /// </summary>
        /// <param name="path">The path</param>
        /// <returns>The parent directory path</returns>
        public static string GetFtpDirectoryName(this string path)
        {
            if (path == null || path.Length == 0 || path.GetFtpPath() == "/")
                return "/";

            return System.IO.Path.GetDirectoryName(path).GetFtpPath();
        }

        /// <summary>
        /// Gets the file name from the path
        /// </summary>
        /// <param name="path">The full path to the file</param>
        /// <returns>The file name</returns>
        public static string GetFtpFileName(this string path)
        {
            return System.IO.Path.GetFileName(path).GetFtpPath();
        }

        /// <summary>
        /// Tries to convert the string FTP date representation  into a date time object
        /// </summary>
        /// <param name="date">The date</param>
        /// <param name="style">UTC/Local Time</param>
        /// <returns>A date time object representing the date, DateTime.MinValue if there was a problem</returns>
        public static DateTime GetFtpDate(this string date, DateTimeStyles style)
        {
            var formats = new[]
                              {
                                  "yyyyMMddHHmmss",
                                  "yyyyMMddHHmmss.fff",
                                  "MMM dd  yyyy",
                                  "MMM dd yyyy",
                                  "MMM dd HH:mm",
                                  "MMM  d HH:mm",
                                  "YYYY-MM-DD-HH:mm"
                              };
            DateTime parsed;

            return DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, style, out parsed) ||
                   DateTime.TryParseExact(date, formats, CultureInfo.CurrentUICulture, style, out parsed)
                       ? parsed
                       : DateTime.MinValue;
        }
    }
}