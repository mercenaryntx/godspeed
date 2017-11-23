using System;
using Microsoft.Practices.Unity;

namespace Neurotoxin.Godspeed.Presentation.Infrastructure
{
    /// <summary>
    /// Provides a singleton Unity instance.
    /// Set by the bootstrapper.
    /// </summary>
    public static class UnityInstance
    {
        private static IUnityContainer container = null;

        public static IUnityContainer Container
        {
            get
            {
                if (container==null)
                    throw new Exception("Container not yet set.");
                return container;
            }
            set
            {
                if (container!=null)
                    throw new Exception("Container already set.");
                if (value==null)
                    throw new ArgumentNullException("value");

                container = value;
            }
        }
    }
}
