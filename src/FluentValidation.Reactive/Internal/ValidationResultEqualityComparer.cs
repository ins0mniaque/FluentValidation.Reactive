using System.Collections.Generic;

using FluentValidation.Results;

namespace FluentValidation.Reactive.Internal
{
    public class ValidationResultEqualityComparer : IEqualityComparer < ValidationResult? >
    {
        public static ValidationResultEqualityComparer Instance { get; } = new ValidationResultEqualityComparer ( );

        public bool Equals ( ValidationResult? left, ValidationResult? right )
        {
            if ( ReferenceEquals ( left,  right ) ) return true;
            if ( ReferenceEquals ( left,  null  ) ) return false;
            if ( ReferenceEquals ( right, null  ) ) return false;

            return RuleSetsExecutedEqualityComparer     .Instance.Equals ( left.RuleSetsExecuted, right.RuleSetsExecuted ) &&
                   ValidationFailureListEqualityComparer.Instance.Equals ( left.Errors,           right.Errors );
        }

        public int GetHashCode ( ValidationResult? validationResult )
        {
            if ( validationResult == null )
                return 0;

            return Hasher.Combine ( RuleSetsExecutedEqualityComparer     .Instance.GetHashCode ( validationResult.RuleSetsExecuted ),
                                    ValidationFailureListEqualityComparer.Instance.GetHashCode ( validationResult.Errors ) );
        }
    }
}