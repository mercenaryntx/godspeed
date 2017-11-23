using System;

namespace Neurotoxin.Godspeed.Core.Exceptions
{
    public class ModelFactoryException : ApplicationException
    {
        public ModelFactoryException(string messageFormat, params object[] args) : 
            base(string.Format(messageFormat, args))
        {
        }

        public ModelFactoryException( Exception innerException, string messageFormat, params object[] args ) :
            base( string.Format( messageFormat, args ), innerException )
        {
        }

    }
}