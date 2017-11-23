using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neurotoxin.Godspeed.Shell.Tests.Helpers;

namespace Neurotoxin.Godspeed.Shell.Tests.Extensions
{
    public static class ListExtensions
    {
        public static T Random<T>(this IList<T> list)
        {
            var index = C.Random<int>(list.Count);
            return list[index];
        }
    }
}