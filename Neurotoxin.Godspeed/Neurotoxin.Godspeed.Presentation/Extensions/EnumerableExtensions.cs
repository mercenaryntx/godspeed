using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Neurotoxin.Godspeed.Presentation.Infrastructure;

namespace Neurotoxin.Godspeed.Presentation.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Gets the index of the first item that matches the given predicate.
        /// Returns -1 if no such item found.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            int index = 0;
            if (items == null) return -1;
            foreach (T item in items)
            {
                if (predicate(item))
                    return index;
                index++;
            }
            return -1;
        }

        public static List<T> ToList<T>(this IList list)
        {
            List<T> output = new List<T>();
            foreach (object o in list) output.Add((T)o);
            return output;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items) where T : class
        {
            var list = items.ToList();
            return new ObservableCollection<T>(list);
        }

        public static IOrderedEnumerable<T> ThenByProperty<T>(this IOrderedEnumerable<T> items, string propertyName, ListSortDirection direction)
        {
            var type = typeof (T);
            var instance = Expression.Parameter(type);
            var callProperty = Expression.PropertyOrField(instance, propertyName);
            var func = typeof(Func<,>).MakeGenericType(type, callProperty.Type);
            var lambda = Expression.Lambda(func, callProperty, instance);
            var methodName = direction == ListSortDirection.Ascending ? "ThenBy" : "ThenByDescending";

            return typeof (Enumerable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
                       .MakeGenericMethod(type, callProperty.Type)
                       .Invoke(null, new object[] {items, lambda.Compile()}) as IOrderedEnumerable<T>;
        }
    }
}