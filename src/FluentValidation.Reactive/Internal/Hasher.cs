using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace FluentValidation.Reactive.Internal
{
    internal static class Hasher
    {
        public static int GetHashCode < T > ( this IEnumerable < T > enumerable, IEqualityComparer < T > equalityComparer )
        {
            unchecked
            {
                return enumerable?.Select ( equalityComparer.GetHashCode ).Aggregate ( 17, (left, right) => 23 * left + right ) ?? 0;
            }
        }

        public static int Combine ( int left, int right )
        {
            unchecked
            {
                return ( left * 397 ) ^ right;
            }
        }
    }
}