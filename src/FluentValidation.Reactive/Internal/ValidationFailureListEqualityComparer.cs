using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using FluentValidation.Results;

namespace FluentValidation.Reactive.Internal
{
    public class ValidationFailureListEqualityComparer : IEqualityComparer < IList < ValidationFailure >? >
    {
        public static ValidationFailureListEqualityComparer Instance { get; } = new ValidationFailureListEqualityComparer ( );

        public bool Equals ( IList < ValidationFailure >? left, IList < ValidationFailure >? right )
        {
            if ( left ?.Count == 0 ) left  = null;
            if ( right?.Count == 0 ) right = null;

            if ( ReferenceEquals ( left,  right ) ) return true;
            if ( ReferenceEquals ( left,  null  ) ) return false;
            if ( ReferenceEquals ( right, null  ) ) return false;
            if ( left.Count != right.Count        ) return false;

            return left.SequenceEqual ( right, ValidationFailureEqualityComparer.Instance );
        }

        public int GetHashCode ( IList < ValidationFailure >? errors )
        {
            return errors?.GetHashCode ( ValidationFailureEqualityComparer.Instance ) ?? 0;
        }
    }
}