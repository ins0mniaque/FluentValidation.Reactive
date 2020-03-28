using System.Collections.Generic;

using FluentValidation.Results;

namespace FluentValidation.Reactive.Internal
{
    public class ValidationFailureEqualityComparer : IEqualityComparer < ValidationFailure? >
    {
        public static ValidationFailureEqualityComparer Instance { get; } = new ValidationFailureEqualityComparer ( );

        public bool Equals ( ValidationFailure? left, ValidationFailure? right )
        {
            if ( ReferenceEquals ( left,  right ) ) return true;
            if ( ReferenceEquals ( left,  null  ) ) return false;
            if ( ReferenceEquals ( right, null  ) ) return false;

            return left.Severity     == right.Severity     &&
                   left.PropertyName == right.PropertyName &&
                   left.ErrorCode    == right.ErrorCode    &&
                   left.ErrorMessage == right.ErrorMessage &&
                   Equals ( left.AttemptedValue, right.AttemptedValue ) &&
                   Equals ( left.CustomState,    right.CustomState    );
        }

        public int GetHashCode ( ValidationFailure? error )
        {
            if ( error == null )
                return 0;

            return Hasher.Combine ( error.Severity       .GetHashCode ( ),
                   Hasher.Combine ( error.PropertyName  ?.GetHashCode ( ) ?? 0,
                   Hasher.Combine ( error.ErrorCode     ?.GetHashCode ( ) ?? 0,
                   Hasher.Combine ( error.ErrorMessage  ?.GetHashCode ( ) ?? 0,
                   Hasher.Combine ( error.AttemptedValue?.GetHashCode ( ) ?? 0,
                                    error.CustomState   ?.GetHashCode ( ) ?? 0 ) ) ) ) );
        }
    }
}