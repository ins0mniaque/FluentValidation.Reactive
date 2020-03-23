using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public static class ObservableExtensions
    {
        public static IObservable < bool > IsValid ( this IObservable < ValidationResult > validationResult )
        {
            return validationResult.Select ( validation => validation.IsValid ).DistinctUntilChanged ( );
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

        public static IObservable < ValidationResult > OfSeverity ( this IObservable < ValidationResult > validationResult, params Severity [ ] severities )
        {
            return validationResult.Select ( validation => validation.OfSeverity ( severities ) )
                                   .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance );
        }

        public static IDisposable Subscribe ( this IObservable < ValidationResult > validationResult, Action < IEnumerable < ValidationFailure > > action )
        {
            return validationResult.Subscribe ( validation => action ( validation.Errors ) );
        }

        public static IDisposable Subscribe ( this IObservable < ValidationResult > validationResult, Action < IEnumerable < ValidationFailure > > onAdd, Action < IEnumerable < ValidationFailure > > onRemove )
        {
            return validationResult.Subscribe ( validation => onAdd    ( validation.Errors ),
                                                validation => onRemove ( validation.Errors ) );
        }

        public static IDisposable Subscribe ( this IObservable < ValidationResult > validationResult, Action < ValidationFailure > onAdd, Action < ValidationFailure > onRemove )
        {
            return validationResult.Subscribe ( validation => validation.Errors.ForEach ( onAdd    ),
                                                validation => validation.Errors.ForEach ( onRemove ) );
        }

        public static IDisposable Subscribe < T > ( this IObservable < ValidationResult > validationResult, Func < ValidationFailure, T > selector, Action < T > onAdd, Action < T > onRemove )
        {
            return validationResult.Subscribe ( validation => validation.Errors.Select ( selector ).ToList ( ),
                                                projection => projection.ForEach ( onAdd    ),
                                                projection => projection.ForEach ( onRemove ) );
        }

        public static IDisposable Subscribe ( this IObservable < ValidationResult > validationResult, Action < ValidationResult > onAdd, Action < ValidationResult > onRemove )
        {
            return validationResult.Select    ( validation => validation.ObservableSubscribe ( validation => validation, onAdd, onRemove ) )
                                   .Switch    ( )
                                   .Subscribe ( );
        }

        public static IDisposable Subscribe < T > ( this IObservable < ValidationResult > validationResult, Func < ValidationResult, T > selector, Action < T > onAdd, Action < T > onRemove )
        {
            return validationResult.Select    ( validation => validation.ObservableSubscribe ( selector, onAdd, onRemove ) )
                                   .Switch    ( )
                                   .Subscribe ( );
        }

        private static IObservable < Unit > ObservableSubscribe < T > ( this ValidationResult validation, Func < ValidationResult, T > selector, Action < T > onAdd, Action < T > onRemove )
        {
            if ( validation.IsValid || validation.Errors.Count == 0 )
                return Observable.Empty < Unit > ( );

            var projection = selector ( validation );

            return Observable.Create < Unit > ( _ =>
            {
                onAdd ( projection );

                return Disposable.Create ( projection, projection => onRemove ( projection ) );
            } );
        }

        private static void ForEach < T > ( this IEnumerable < T > enumerable, Action < T > action )
        {
            foreach ( var item in enumerable )
                action ( item );
        }
    }
}