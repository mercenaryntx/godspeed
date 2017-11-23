using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Neurotoxin.Godspeed.Core.Extensions
{
	public static class EnumerableExtensions
	{
		public static IList<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
		{
			IEnumerable<IEnumerable<T>> enumerables = new[] { Enumerable.Empty<T>() };
			return sequences.Aggregate(enumerables, ( accumulator, sequence) => 
				from accseq in accumulator
				from item in sequence
				select accseq.Concat(new[] { item })).ToList();
		}

        public static int IndexOf<T>(this IEnumerable<T> items, Func<T,bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            var retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
	}
}