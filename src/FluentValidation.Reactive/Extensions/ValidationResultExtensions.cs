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
        public static ValidationResult For < T > ( this ValidationResult validationResult, Expression < Func < T, object > > property )
        {
            return validationResult.For ( PropertyChain.FromExpression ( property ).ToString ( ), true );
        }

        public static ValidationResult For < T > ( this ValidationResult validationResult, params Expression < Func < T, object > > [ ] properties )
        {
            return validationResult.For ( properties.Select ( property => PropertyChain.FromExpression ( property ).ToString ( ) ).ToArray ( ), true );
        }

        public static ValidationResult StrictlyFor < T > ( this ValidationResult validationResult, Expression < Func < T, object > > property )
        {
            return validationResult.For ( PropertyChain.FromExpression ( property ).ToString ( ), false );
        }

        public static ValidationResult StrictlyFor < T > ( this ValidationResult validationResult, params Expression < Func < T, object > > [ ] properties )
        {
            return validationResult.For ( properties.Select ( property => PropertyChain.FromExpression ( property ).ToString ( ) ).ToArray ( ), false );
        }

        public static ValidationResult For ( this ValidationResult validationResult, string propertyPath )             => validationResult.For ( propertyPath,  true );
        public static ValidationResult For ( this ValidationResult validationResult, params string [ ] propertyPaths ) => validationResult.For ( propertyPaths, true );

        public static ValidationResult StrictlyFor ( this ValidationResult validationResult, string propertyPath )             => validationResult.For ( propertyPath,  false );
        public static ValidationResult StrictlyFor ( this ValidationResult validationResult, params string [ ] propertyPaths ) => validationResult.For ( propertyPaths, false );

        private static ValidationResult For ( this ValidationResult validationResult, string propertyPath, bool includeChildProperties )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            return new ValidationResult ( validationResult.Errors.Where ( error => error.IsFor ( propertyPath, includeChildProperties ) ) );
        }

        private static ValidationResult For ( this ValidationResult validationResult, string [ ] propertyPaths, bool includeChildProperties )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            return new ValidationResult ( validationResult.Errors.Where ( error => propertyPaths.Any ( propertyPath => error.IsFor ( propertyPath, includeChildProperties ) ) ) );
        }

        private static bool IsFor ( this ValidationFailure error, string propertyPath, bool includeChildProperties )
        {
            propertyPath ??= string.Empty;

            var propertyName = error.PropertyName ?? string.Empty;
            if ( propertyName == propertyPath )
                return true;

            if ( ! includeChildProperties )
                return false;

            return propertyName.StartsWith ( propertyPath, StringComparison.Ordinal ) &&
                   ! char.IsLetter ( propertyName [ propertyPath.Length ] );
        }

        public static Severity? GetSeverity ( this ValidationResult validationResult )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            if ( validationResult == null || validationResult.IsValid )
                return null;

            return validationResult.Errors.Select ( error => error.Severity ).Min ( );
        }

        public static ValidationResult OfSeverity ( this ValidationResult validationResult, params Severity [ ] severities )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            return new ValidationResult ( validationResult.Errors.Where ( error => severities.Contains ( error.Severity ) ) );
        }
    }
}