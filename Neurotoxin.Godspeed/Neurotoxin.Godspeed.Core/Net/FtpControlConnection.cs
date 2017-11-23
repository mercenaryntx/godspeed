using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ServiceStack.Text;

namespace Neurotoxin.Godspeed.Core.Net
{
    internal class FtpControlConnection : IDisposable
    {
        private FtpSocketStream _socketStream;
        private StreamReader _streamReader;

        public string Host { get; set; }
        public int Port { get; set; }
        public FtpIpVersion IpVersion { get; set; }

        public bool IsConnected
        {
            get { return _socketStream.IsConnected; }
        }

        public int SocketPollInterval
        {
            get { return _socketStream.SocketPollInterval; }
            set { _socketStream.SocketPollInterval = value; }
        }

        public int ReadTimeout
        {
            get { return _socketStream.ReadTimeout; }
            set { _socketStream.ReadTimeout = value; }
        }

        public int SocketDataAvailable
        {
            get { return _socketStream.SocketDataAvailable; }
        }

        public bool IsEncrypted
        {
            get { return _socketStream.IsEncrypted; }
        }

        public int ConnectTimeout
        {
            get { return _socketStream.ConnectTimeout; }
            set { _socketStream.ConnectTimeout = value; }
        }

        public Encoding TextEncoding { get; set; }

        public FtpEncryptionMode EncryptionMode { get; set; }

        public bool KeepAlive { get; set; }

        public X509CertificateCollection ClientCertificates { get; set; }

        public IPEndPoint LocalEndPoint
        {
            get { return _socketStream.LocalEndPoint; }
        }

        internal FtpControlConnection(Action<FtpSocketStream, FtpSslValidationEventArgs> validateCertificateHandler) : this(new FtpSocketStream())
        {
            _socketStream.ValidateCertificate += new FtpSocketStreamSslValidation(validateCertificateHandler);
        }

        internal FtpControlConnection(FtpSocketStream socketStream)
        {
            _socketStream = socketStream;
        }

        public void Connect()
        {
            _socketStream.Connect(Host, Port, IpVersion);
            _socketStream.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, KeepAlive);
            if (EncryptionMode == FtpEncryptionMode.Implicit)
                _socketStream.ActivateEncryption(Host, ClientCertificates.Count > 0 ? ClientCertificates : null);
            _streamReader = new StreamReader(_socketStream);
        }

        public void ActivateEncryption(string targethost, X509CertificateCollection clientCerts)
        {
            _socketStream.ActivateEncryption(targethost, clientCerts);
        }

        public FtpReply ReadLine()
        {
            return new FtpReply(ReadRawLine());
        }

        public void ReadLineAsync(Action<FtpReply> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");

            Func<string> w = ReadRawLine;
            w.BeginInvoke(asyncResult =>
            {
                FtpReply result = null;
                try
                {
                    result = new FtpReply(w.EndInvoke(asyncResult));
                }
                finally
                {
                    callback.Invoke(result);
                }
            }, null);
        }

        public string ReadRawLine()
        {
            var raw = _streamReader.ReadLine();
            FtpTrace.WriteLine(raw);
            return raw;
        }

        public bool ReadRawLine(out string raw)
        {
            raw = ReadRawLine();
            var line = raw.Trim().ToUpper();
            return line.EndsWith("END") || line.StartsWith("220 ");
        }

        public void SetSocketOption(SocketOptionLevel level, SocketOptionName name, bool value)
        {
            _socketStream.SetSocketOption(level, name, value);
        }

        public void RawSocketRead(byte[] buffer)
        {
            _socketStream.RawSocketRead(buffer);
        }

        public FtpReply Execute(string command, params object[] args)
        {
            return Execute(string.Format(command, args));
        }

        public FtpReply Execute(string command)
        {
            FtpTrace.WriteLine(command.StartsWith("PASS") ? "PASS <omitted>" : command);
            _socketStream.WriteLine(TextEncoding, command);
            return GetReply(command.StartsWith("FEAT")); //TODO: Aurora hack
        }

        internal FtpReply GetReply(bool multipart)
        {
            if (!IsConnected)
                throw new InvalidOperationException("No connection to the server has been established.");

            var reply = ReadLine();
            if (multipart || reply.IsMultipart)
            {
                var sb = new StringBuilder();
                string line;
                while (!ReadRawLine(out line))
                {
                    if (sb.Length != 0) sb.Append(Environment.NewLine);
                    sb.Append(line);
                }
                reply.InfoMessages = sb.ToString().TrimEnd();
            }

            return reply;
        }

        public void Close()
        {
            _streamReader.Close();
        }

        public void Dispose()
        {
            Close();
            _streamReader.Dispose();
            _socketStream.Dispose();
            _streamReader = null;
            _socketStream = null;
        }
    }
}