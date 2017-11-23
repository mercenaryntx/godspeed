using System.Reflection;
using System.Runtime.CompilerServices;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    /// <summary>
    /// Extension methods to the System.Reflection.MethodInfo class
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Determines that the given MethodInfo reflects to a real method or not (i.e. property accessors)
        /// </summary>
        /// <param name="method">The method info</param>
        /// <returns>True if it's a real method, False if not</returns>
        public static bool IsMethod(this MethodInfo method)
        {
            var methodPrefix = method.Name.Substring(0, 4);
            return !methodPrefix.Equals("get_") && !methodPrefix.Equals("set_");
        }

        public static bool IsGetMethod(this MethodInfo method)
        {
            var methodPrefix = method.Name.Substring(0, 4);
            return methodPrefix.Equals("get_");
        }

        /// <summary>
        /// Get the method where is it declared
        /// </summary>
        /// <param name="method">The method info</param>
        /// <returns>The method info where is it declared</returns>
        public static MethodInfo GetMethod(this MethodInfo method)
        {
            return method.DeclaringType.GetMethod(method.Name,
                                                  BindingFlags.Instance |
                                                  BindingFlags.Public |
                                                  BindingFlags.NonPublic);
        }

        /// <summary>
        /// Get the property from a property accessor
        /// </summary>
        /// <param name="method">The property accessor</param>
        /// <returns>The property</returns>
        public static PropertyInfo GetProperty(this MethodInfo method)
        {
            if (method.IsMethod()) return null;
            var type = method.DeclaringType;
            return type.GetProperty(method.Name.Substring(4),
                                            BindingFlags.Instance |
                                            BindingFlags.Public |
                                            BindingFlags.NonPublic);
        }

        /// <summary>
        /// Determines that whether the given property accessor belongs to an auto-property or not
        /// </summary>
        /// <param name="method">The property accessor</param>
        /// <returns>True if it's a getter or setter of an auto-property, False if not</returns>
        public static bool IsAutoProperty(this MethodInfo method)
        {
            return method.IsDefined(typeof(CompilerGeneratedAttribute), false);
        }

    }
}