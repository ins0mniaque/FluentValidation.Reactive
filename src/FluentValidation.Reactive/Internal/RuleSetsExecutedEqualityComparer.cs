using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace FluentValidation.Reactive.Internal
{
    public class RuleSetsExecutedEqualityComparer : IEqualityComparer < string [ ]? >
    {
        public static RuleSetsExecutedEqualityComparer Instance { get; } = new RuleSetsExecutedEqualityComparer ( );

        public bool Equals ( string [ ]? left, string [ ]? right )
        {
            if ( left ?.Length == 0 ) left  = null;
            if ( right?.Length == 0 ) right = null;

            if ( ReferenceEquals ( left,  right ) ) return true;
            if ( ReferenceEquals ( left,  null  ) ) return false;
            if ( ReferenceEquals ( right, null  ) ) return false;
            if ( left.Length != right.Length      ) return false;

            return left.SequenceEqual ( right, StringComparer.Ordinal );
        }

        public int GetHashCode ( string [ ]? error )
        {
            return error?.GetHashCode ( StringComparer.Ordinal ) ?? 0;
        }
    }
}