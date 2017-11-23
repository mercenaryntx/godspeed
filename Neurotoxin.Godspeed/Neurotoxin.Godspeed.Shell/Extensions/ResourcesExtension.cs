using System;
using System.Resources;
using Resx = Neurotoxin.Godspeed.Shell.Properties.Resources;

namespace Neurotoxin.Godspeed.Shell.Extensions
{
    public static class ResourcesExtension
    {
        internal static string EnumToTranslation(this ResourceManager r, Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            var key = type.Name + name;
            return r.GetString(key) ?? "Key: " + key;
        }
    }
}