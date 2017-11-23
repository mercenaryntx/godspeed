using System;
using ServiceStack.Logging;

namespace Neurotoxin.Godspeed.Shell.Database
{
    public class OrmLiteLogFactory : ILogFactory
    {
        private readonly bool _debugEnabled;

        public OrmLiteLogFactory(bool debugEnabled = true)
        {
            _debugEnabled = debugEnabled;
        }

        public ILog GetLogger(Type type)
        {
            return new OrmLiteLogger(_debugEnabled);
        }

        public ILog GetLogger(string typeName)
        {
            return new OrmLiteLogger(_debugEnabled);
        }
    }
}