using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;

using FluentValidation;
using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public static class ReactiveValidatableExtensions
    {
        public static void Validate < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            reactiveValidatable.ReactiveValidator.Instance = reactiveValidatable;

            reactiveValidatable.ReactiveValidator.Validate ( );
        }

        public static IObservable < ValidationResult > ValidationResult < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult;
        }

        public static IObservable < ValidationResult > ValidationResult < T > ( this IObservable < T > reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.Where  ( validatable => validatable != null )
                                      .Select ( validatable => validatable.ReactiveValidator.ValidationResult )
                                      .Switch ( );
        }

        public static IObservable < ValidationResult > ValidationResult < T > ( this IObservable < T > reactiveValidatable, Func < ValidationResult, ValidationResult > transform ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.Where  ( validatable => validatable != null )
                                      .Select ( validatable => validatable.ReactiveValidator.ValidationResult.Select ( transform ) )
                                      .Switch ( );
        }

        public static IObservable < ValidationResult > ValidationResultFor < T, TProperty > ( this IObservable < T > reactiveValidatable, Expression < Func < T, TProperty > > property, bool includeChildProperties = true ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( validation => validation.For ( property, includeChildProperties ) );
        }

        public static IObservable < bool > IsValid < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.IsValid ( );
        }

        public static IObservable < bool > IsValidFor < T, TProperty > ( this T reactiveValidatable, Expression < Func < T, TProperty > > property, bool includeChildProperties = true ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.For ( property, includeChildProperties ).IsValid ( );
        }

        public static IObservable < Severity? > Severity < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.Severity ( );
        }

        public static IObservable < Severity? > SeverityFor < T, TProperty > ( this T reactiveValidatable, Expression < Func < T, TProperty > > property, bool includeChildProperties = true ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.For ( property, includeChildProperties ).Severity ( );
        }

        public static IDisposable ValidateWhen < T, TSignal > ( this T reactiveValidatable, IObservable < TSignal > signal ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            reactiveValidatable.ReactiveValidator.Instance = reactiveValidatable;

            return signal.Subscribe ( _ => reactiveValidatable.ReactiveValidator.Validate ( ) );
        }
    }
}