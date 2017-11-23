using System;
using System.IO;

namespace Neurotoxin.Godspeed.Core.Logging
{
    public static class DebugLogger
    {
        public static void Log(string messageFormat, params object[] args)
        {
            File.AppendAllText("Debug.log", string.Format(messageFormat, args) + Environment.NewLine);
        }
    }
}