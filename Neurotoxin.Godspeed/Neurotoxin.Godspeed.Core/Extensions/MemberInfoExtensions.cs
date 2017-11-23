using System;
using System.Reflection;
using System.Linq;

namespace Neurotoxin.Godspeed.Core.Extensions
{
    /// <summary>
    /// Extension methods to the System.Reflection.MemberInfo class
    /// </summary>
    public static class MemberInfoExtensions
    {
        public static bool HasAttribute<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
        {
            return memberInfo.GetAttribute<T>(inherit) != null;
        }

        /// <summary>
        /// Gets an attribute with the given type
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="memberInfo">The member info</param>
        /// <param name="inherit"></param>
        /// <returns>The desired attribute or null</returns>
        public static T GetAttribute<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
        {
            return (T) Attribute.GetCustomAttribute(memberInfo, typeof (T), inherit);
        }


        /// <summary>
        /// Gets an attribute with the given type
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="assembly">The assembly</param>
        /// <param name="inherit"></param>
        /// <returns>The desired attribute or null</returns>
        public static T GetAttribute<T>(this Assembly assembly, bool inherit = true) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(assembly, typeof(T), inherit);
        }

        /// <summary>
        /// Gets an attribute collection with the given type
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="memberInfo">The member info</param>
        /// <param name="inherit"></param>
        /// <returns>A collection of the desired attributes</returns>
        public static T[] GetAttributes<T>(this MemberInfo memberInfo, bool inherit = true) where T : Attribute
        {
            return Attribute.GetCustomAttributes(memberInfo, typeof (T), inherit).Select(a => (T)a).ToArray();
        }
    }
}