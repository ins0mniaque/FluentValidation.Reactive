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

        public static IObservable < ValidationResult > ValidationResult < T, TProperty > ( this T reactiveValidatable, Expression < Func < T, TProperty > > expression ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.ForProperty ( expression );
        }

        public static IObservable < ValidationResult > ValidationResult < T > ( this IObservable < T > reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.Where  ( validatable => validatable != null )
                                      .Select ( validatable => validatable.ReactiveValidator.ValidationResult )
                                      .Switch ( );
        }

        public static IObservable < ValidationResult > ValidationResult < T, TProperty > ( this IObservable < T > reactiveValidatable, Expression < Func < T, TProperty > > expression ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.Where  ( validatable => validatable != null )
                                      .Select ( validatable => validatable.ReactiveValidator.ValidationResult.ForProperty ( expression ) )
                                      .Switch ( );
        }

        public static IObservable < bool > IsValid < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.IsValid ( );
        }

        public static IObservable < bool > IsValid < T, TProperty > ( this T reactiveValidatable, Expression < Func < T, TProperty > > expression ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.ForProperty ( expression ).IsValid ( );
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