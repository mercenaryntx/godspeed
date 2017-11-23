using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Neurotoxin.Godspeed.Shell.Interfaces;
using System.Linq;

namespace Neurotoxin.Godspeed.Shell.Reporting
{
    public static class HttpForm
    {
        private const string Boundary = "----GODSpeedFormBoundary";
        private const string UrlBase = "http://www.mercenary.hu/godspeed/";
        private const string UserAgent = "GODspeed";
        private const string ContentType = "multipart/form-data; boundary=" + Boundary;

        public static void Post(string target, IEnumerable<IFormData> formData)
        {
            byte[] body = null;
            try
            {
                using (var client = new WebClient())
                {
                    using (var ms = new MemoryStream())
                    {
                        var sw = new StreamWriter(ms);
                        foreach (var data in formData)
                        {
                            sw.WriteLine("--" + Boundary);
                            data.Write(sw);
                        }
                        sw.WriteLine("--" + Boundary + "--");
                        sw.Flush();
                        body = ms.ToArray();
                    }

                    client.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    client.Headers[HttpRequestHeader.ContentType] = ContentType;
                    client.UploadData(new Uri(UrlBase + target), body);
                }
            }
            catch (WebException)
            {
                if (body == null) return;
                var dirName = string.Format(@"{0:yyyyMMddHHmmssffff}", DateTime.Now);
                var dir = Path.Combine(App.PostDirectory, dirName);
                Directory.CreateDirectory(dir);
                var tempFile = Path.Combine(dir, target);
                File.WriteAllBytes(tempFile, body);
            }
            catch
            {

            }
        }

        public static void Post(string target, IEnumerable<KeyValuePair<string, string>> formData)
        {
            Post(target, formData.Select(kvp => new RawPostData(kvp.Key, kvp.Value)));
        }

        public static bool Repost(string path)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var body = File.ReadAllBytes(path);
                    var target = Path.GetFileName(path);
                    client.Headers[HttpRequestHeader.UserAgent] = UserAgent;
                    client.Headers[HttpRequestHeader.ContentType] = ContentType;
                    client.UploadData(new Uri(UrlBase + target), body);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}