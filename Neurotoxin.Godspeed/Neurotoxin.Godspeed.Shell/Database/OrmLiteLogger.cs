using System;
using ServiceStack.Logging;

namespace Neurotoxin.Godspeed.Shell.Database
{
    public class OrmLiteLogger : ILog
    {
        private const string DEBUG = "DEBUG: ";
        private const string ERROR = "ERROR: ";
        private const string FATAL = "FATAL: ";
        private const string INFO = "INFO: ";
        private const string WARN = "WARN: ";

        public bool IsDebugEnabled { get; private set; }

        public OrmLiteLogger(bool isDebugEnabled)
        {
            IsDebugEnabled = isDebugEnabled;
        }

        public void Debug(object message, Exception exception)
        {
            Log(string.Concat(DEBUG, message), exception);
        }

        public void Debug(object message)
        {
            Log(string.Concat(DEBUG, message));
        }

        public void DebugFormat(string format, params object[] args)
        {
            LogFormat(string.Concat(DEBUG, format), args);
        }

        public void Error(object message, Exception exception)
        {
            Log(string.Concat(ERROR, message), exception);
        }

        public void Error(object message)
        {
            Log(string.Concat(ERROR, message));
        }

        public void ErrorFormat(string format, params object[] args)
        {
            LogFormat(string.Concat(ERROR, format), args);
        }

        public void Fatal(object message, Exception exception)
        {
            Log(string.Concat(FATAL, message), exception);
        }

        public void Fatal(object message)
        {
            Log(string.Concat(FATAL, message));
        }

        public void FatalFormat(string format, params object[] args)
        {
            LogFormat(string.Concat(FATAL, format), args);
        }

        public void Info(object message, Exception exception)
        {
            Log(string.Concat(INFO, message), exception);
        }

        public void Info(object message)
        {
            Log(string.Concat(INFO, message));
        }

        public void InfoFormat(string format, params object[] args)
        {
            LogFormat(string.Concat(INFO, format), args);
        }

        private static void Log(object message, Exception exception)
        {
            string str = (message == null ? string.Empty : message.ToString());
            if (exception != null)
            {
                str = string.Concat(str, ", Exception: ", exception.Message);
                System.Diagnostics.Debug.WriteLine(str);
            }
        }

        private static void Log(object message)
        {
            if (message == null) return;
            System.Diagnostics.Debug.WriteLine(message);
        }

        private static void LogFormat(object message, params object[] args)
        {
            if (message == null) return;
            System.Diagnostics.Debug.WriteLine(message.ToString(), args);
        }

        public void Warn(object message, Exception exception)
        {
            Log(string.Concat(WARN, message), exception);
        }

        public void Warn(object message)
        {
            Log(string.Concat(WARN, message));
        }

        public void WarnFormat(string format, params object[] args)
        {
            LogFormat(string.Concat(WARN, format), args);
        }
    }
}