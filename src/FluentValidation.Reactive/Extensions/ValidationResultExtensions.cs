using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;

using FluentValidation.Internal;
using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public static class ValidationResultExtensions
    {
        public static Severity? GetSeverity ( this ValidationResult validationResult )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            if ( validationResult == null || validationResult.IsValid )
                return null;

            return validationResult.Errors.Select ( error => error.Severity ).Min ( );
        }

        public static ValidationResult For < T, TProperty > ( this ValidationResult validationResult, Expression < Func < T, TProperty > > property, bool includeChildProperties = true )
        {
            return validationResult.For ( PropertyChain.FromExpression ( property ).ToString ( ), includeChildProperties );
        }

        public static ValidationResult For ( this ValidationResult validationResult, string propertyPath, bool includeChildProperties = true )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            propertyPath ??= string.Empty;

            return new ValidationResult ( validationResult.Errors.Where ( Matches ) );

            bool Matches ( ValidationFailure error )
            {
                var propertyName = error.PropertyName ?? string.Empty;
                if ( propertyName == propertyPath )
                    return true;

                if ( ! includeChildProperties )
                    return false;

                return propertyName.StartsWith ( propertyPath, StringComparison.Ordinal ) &&
                       ! char.IsLetter ( propertyName [ propertyPath.Length ] );
            }
        }

        public static ValidationResult OfSeverity ( this ValidationResult validationResult, params Severity [ ] severities )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            return new ValidationResult ( validationResult.Errors.Where ( error => severities.Contains ( error.Severity ) ) );
        }
    }
}