using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Neurotoxin.Godspeed.Core.Exceptions;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Net
{
    public static class Telnet
    {
        private static string _networkDrive;
        private static TcpClient _client;
        private static NetworkStream _ns;
        private static string _rootPath;
        private static long _totalSize;
        private static long _totalTransferred;

        private static readonly Regex ShareParser = new Regex(@"^\\\\(?<host>.*?)\\(?<sharename>.*?)(?<directory>\\.*)?$");
        private static readonly Regex RootPathParser = new Regex(@"path\s*=\s*(?<path>.*)\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static readonly Regex ProgressParser = new Regex(@"' at (?<totalTransferred>[0-9]+)", RegexOptions.Multiline);
        private static readonly Regex FinishParser = new Regex(@"(?<totalTransferred>[0-9]+) bytes transferred", RegexOptions.Multiline);

        public static void OpenSession(string networkDrive, string sambaShare, string ftpHost, int port, string ftpUser, string ftpPwd)
        {
            if (_client != null) throw new TelnetException(null, "A telnet session is already opened. Please close the current one first before starting a new one.");

            _networkDrive = networkDrive;
            var shareParts = ShareParser.Match(sambaShare);
            if (!shareParts.Success)
                throw new TelnetException("?", "Invalid UNC path: {0}.", sambaShare);
            var host = shareParts.Groups["host"].Value;
            var shareName = shareParts.Groups["sharename"].Value;
            var directory = shareParts.Groups["directory"].Value;

            _client = Connect(host);
            _ns = _client.GetStream();

            _rootPath = null;
            WaitForCursor();
            Send(string.Format("sed -e '/{0}/,/path/!d' /etc/samba/smb.conf", shareName), s =>
                {
                    var m = RootPathParser.Match(s);
                    if (m.Success)
                    {
                        _rootPath = m.Groups["path"].Value.Trim();
                        if (!string.IsNullOrEmpty(directory))
                        {
                            _rootPath = _rootPath.TrimEnd('/') + directory.Replace(@"\", "/");
                        }
                        if (!_rootPath.EndsWith("/")) _rootPath += "/";
                    }
                });
            if (string.IsNullOrEmpty(_rootPath))
                throw new TelnetException(host, "Samba share not found.");

            Send("lftp");
            Send(string.Format("open -p {0} {1}", port, ftpHost));
            if (!string.IsNullOrEmpty(ftpUser))
            {
                Send(string.Format("user {0} {1}", ftpUser, ftpPwd));
            }
        }

        private static TcpClient Connect(string host)
        {
            var timeoutObject = new ManualResetEvent(false);
            var connected = false;
            Exception exception = null;
            var client = new TcpClient();
            client.BeginConnect(host, 23, ar =>
                {
                    try
                    {
                        connected = false;
                        if (client.Client != null)
                        {
                            client.EndConnect(ar);
                            connected = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        connected = false;
                        exception = ex;
                    }
                    finally
                    {
                        timeoutObject.Set();
                    }
                }, null);

            if (timeoutObject.WaitOne(5000, false))
            {
                if (connected) return client;
                throw new TelnetException(host, exception.Message);
            }

            client.Close();
            throw new TelnetException(host, "The connection timed out.");
        }

        public static void CloseSession()
        {
            Send("exit");
            _client.Close();
            _ns = null;
            _client = null;
        }

        public static void ChangeFtpDirectory(string path)
        {
            Send(string.Format("cd {0}", path));
        }

        public static void Download(string ftpFileName, string targetPath, long size, long resumeStartPosition, Action<int, long, long, long> progressChanged)
        {
            _totalSize = size;
            _totalTransferred = 0;
            var target = SanitizePath(targetPath);
            progressChanged.Invoke(-1, resumeStartPosition, resumeStartPosition, resumeStartPosition);
            Send(string.Format("get -c {0} -o {1}{2}", ftpFileName, _rootPath, target), s => NotifyProgressChange(s, resumeStartPosition, progressChanged));
        }

        public static void Upload(string ftpFileName, string sourcePath, long size, long resumeStartPosition, Action<int, long, long, long> progressChanged)
        {
            _totalSize = size;
            _totalTransferred = 0;
            var source = SanitizePath(sourcePath);
            progressChanged.Invoke(-1, resumeStartPosition, resumeStartPosition, resumeStartPosition);
            Send(string.Format("put -c {0}{1} -o {2}", _rootPath, source, ftpFileName), s => NotifyProgressChange(s, resumeStartPosition, progressChanged));
        }

        private static string SanitizePath(string path)
        {
            return path.Replace(_networkDrive, string.Empty).Replace(@"\", "/").Replace(" ", @"\ ").Replace("&", @"\&");
        }

        private static void NotifyProgressChange(string s, long resumeStartPosition, Action<int, long, long, long> progressChanged)
        {
            var summary = FinishParser.Match(s);
            if (summary.Success)
            {
                var t = Int64.Parse(summary.Groups["totalTransferred"].Value);
                var transferred = t - _totalTransferred;
                _totalTransferred = t;
                progressChanged.Invoke(100, transferred, _totalTransferred, resumeStartPosition);
            }
            else
            {
                var progress = ProgressParser.Matches(s).Cast<Match>().LastOrDefault();
                if (progress != null)
                {
                    var t = Int64.Parse(progress.Groups["totalTransferred"].Value);
                    var transferred = t - _totalTransferred;
                    _totalTransferred = t;
                    var percentage = (int)(_totalTransferred * 100 / _totalSize);
                    progressChanged.Invoke(percentage, transferred, _totalTransferred, resumeStartPosition);
                }
            }
                
        }

        private static string Read()
        {
            var sb = new StringBuilder();
            if (_ns.CanRead)
            {
                var readBuffer = new byte[1024];
                while (_ns.DataAvailable)
                {
                    var numBytesRead = _ns.Read(readBuffer, 0, readBuffer.Length);
                    var data = Encoding.ASCII.GetString(readBuffer, 0, numBytesRead);
                    data = data.Replace(Convert.ToChar(24), ' ');
                    data = data.Replace(Convert.ToChar(255), ' ');
                    data = data.Replace('?', ' ');
                    sb.AppendFormat("{0}", data);
                }
            }
            Thread.Sleep(100);
            return sb.ToString();
        }

        private static void Send(string message, Action<string> processResponse = null)
        {
            var msg = Encoding.ASCII.GetBytes(message + Environment.NewLine);
            _ns.Write(msg, 0, msg.Length);
            WaitForCursor(processResponse);
        }

        private static void WaitForCursor(Action<string> processResponse = null)
        {
            string x;
            do
            {
                x = Read().Trim();
                if (string.IsNullOrEmpty(x)) continue;
                Debug.WriteLine(x);
                if (processResponse != null) processResponse.Invoke(x);
            }
            while (!x.EndsWith("#") && !x.EndsWith(">"));
        }
    }
}