using System;

namespace Neurotoxin.Godspeed.Core.Exceptions
{
    public class TelnetException : ApplicationException
    {
        public string Host { get; private set; }

        public TelnetException(string host, string messageFormat, params object[] args) : base(string.Format(messageFormat, args))
        {
            Host = host;
        }

        public TelnetException(string host, Exception innerException, string messageFormat, params object[] args ) : base( string.Format( messageFormat, args ), innerException )
        {
            Host = host;
        }
    }
}
