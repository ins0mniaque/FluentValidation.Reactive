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
        public static IObservable < bool > IsValid ( this IObservable < ValidationResult > validationResult )
        {
            return validationResult.Select ( validation => validation.IsValid ).DistinctUntilChanged ( );
        }

        public static IObservable < ValidationResult > For < T > ( this IObservable < ValidationResult > validationResult, Expression < Func < T, object > > property )
        {
            return validationResult.Select ( validation => validation.For ( property ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IObservable < ValidationResult > For < T > ( this IObservable < ValidationResult > validationResult, params Expression < Func < T, object > > [ ] properties )
        {
            return validationResult.Select ( validation => validation.For ( properties ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IObservable < ValidationResult > StrictlyFor < T > ( this IObservable < ValidationResult > validationResult, Expression < Func < T, object > > property )
        {
            return validationResult.Select ( validation => validation.StrictlyFor ( property ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IObservable < ValidationResult > StrictlyFor < T > ( this IObservable < ValidationResult > validationResult, params Expression < Func < T, object > > [ ] properties )
        {
            return validationResult.Select ( validation => validation.StrictlyFor ( properties ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IObservable < ValidationResult > For ( this IObservable < ValidationResult > validationResult, string propertyPath )
        {
            return validationResult.Select ( validation => validation.For ( propertyPath ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IObservable < ValidationResult > For ( this IObservable < ValidationResult > validationResult, params string [ ] propertyPaths )
        {
            return validationResult.Select ( validation => validation.For ( propertyPaths ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IObservable < ValidationResult > StrictlyFor ( this IObservable < ValidationResult > validationResult, string propertyPath )
        {
            return validationResult.Select ( validation => validation.StrictlyFor ( propertyPath ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IObservable < ValidationResult > StrictlyFor ( this IObservable < ValidationResult > validationResult, params string [ ] propertyPaths )
        {
            return validationResult.Select ( validation => validation.StrictlyFor ( propertyPaths ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

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

        public static IObservable < Severity? > Severity ( this IObservable < ValidationResult > validationResult )
        {
            return validationResult.Select ( validation => validation.GetSeverity ( ) ).DistinctUntilChanged ( );
        }

        public static IObservable < bool > Is ( this IObservable < Severity? > severity, params Severity [ ] severities )
        {
            return severity.Select ( severity => severity != null && severities.Contains ( severity.Value ) ).DistinctUntilChanged ( );
        }

        public static IObservable < bool > IsNot ( this IObservable < Severity? > severity, params Severity [ ] severities )
        {
            return severity.Select ( severity => severity == null || ! severities.Contains ( severity.Value ) ).DistinctUntilChanged ( );
        }

        public static IObservable < ValidationResult > OfSeverity ( this IObservable < ValidationResult > validationResult, params Severity [ ] severities )
        {
            return validationResult.Select ( validation => validation.OfSeverity ( severities ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static ValidationResult OfSeverity ( this ValidationResult validationResult, params Severity [ ] severities )
        {
            if ( validationResult == null )
                throw new ArgumentNullException ( nameof ( validationResult ) );

            return new ValidationResult ( validationResult.Errors.Where ( error => severities.Contains ( error.Severity ) ) );
        }
    }
}