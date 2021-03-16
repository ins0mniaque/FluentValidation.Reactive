using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading;

using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public static class ReactiveValidatableExtensions
    {
        public static void Validate < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            reactiveValidatable.ReactiveValidator.Validate ( reactiveValidatable );
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

        public static IObservable < ValidationResult > ValidationResultFor < T > ( this T reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( ).For ( property );
        }

        public static IObservable < ValidationResult > ValidationResultFor < T > ( this T reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( ).For ( properties );
        }

        public static IObservable < ValidationResult > ValidationResultStrictlyFor < T > ( this T reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( ).StrictlyFor ( property );
        }

        public static IObservable < ValidationResult > ValidationResultStrictlyFor < T > ( this T reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( ).StrictlyFor ( properties );
        }

        public static IObservable < ValidationResult > ValidationResultFor < T > ( this IObservable < T > reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( validation => validation.For ( property ) );
        }

        public static IObservable < ValidationResult > ValidationResultFor < T > ( this IObservable < T > reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( validation => validation.For ( properties ) );
        }

        public static IObservable < ValidationResult > ValidationResultStrictlyFor < T > ( this IObservable < T > reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( validation => validation.StrictlyFor ( property ) );
        }

        public static IObservable < ValidationResult > ValidationResultStrictlyFor < T > ( this IObservable < T > reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            return reactiveValidatable.ValidationResult ( validation => validation.StrictlyFor ( properties ) );
        }

        public static IObservable < bool > IsValid < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.IsValid ( );
        }

        public static IObservable < bool > IsValidFor < T > ( this T reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.For ( property ).IsValid ( );
        }

        public static IObservable < bool > IsValidFor < T > ( this T reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.For ( properties ).IsValid ( );
        }

        public static IObservable < bool > IsValidStrictlyFor < T > ( this T reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.StrictlyFor ( property ).IsValid ( );
        }

        public static IObservable < bool > IsValidStrictlyFor < T > ( this T reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.StrictlyFor ( properties ).IsValid ( );
        }

        public static IObservable < Severity? > Severity < T > ( this T reactiveValidatable ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.Severity ( );
        }

        public static IObservable < Severity? > SeverityFor < T > ( this T reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.For ( property ).Severity ( );
        }

        public static IObservable < Severity? > SeverityFor < T > ( this T reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.For ( properties ).Severity ( );
        }

        public static IObservable < Severity? > SeverityStrictlyFor < T > ( this T reactiveValidatable, Expression < Func < T, object? > > property ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.StrictlyFor ( property ).Severity ( );
        }

        public static IObservable < Severity? > SeverityStrictlyFor < T > ( this T reactiveValidatable, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ReactiveValidator.ValidationResult.StrictlyFor ( properties ).Severity ( );
        }

        public static IDisposable ValidateWhen < T, TSignal > ( this T reactiveValidatable, IObservable < TSignal > signal ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return signal.Subscribe ( _ => reactiveValidatable.ReactiveValidator.Validate ( reactiveValidatable ) );
        }

        public static IDisposable ValidateWhen < T, TSignal > ( this T reactiveValidatable, IObservable < TSignal > signal, params string [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return signal.Subscribe ( _ => reactiveValidatable.ReactiveValidator.Validate ( reactiveValidatable, CancellationToken.None, properties ) );
        }

        public static IDisposable ValidateWhen < T, TSignal > ( this T reactiveValidatable, IObservable < TSignal > signal, params Expression < Func < T, object? > > [ ] properties ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return signal.Subscribe ( _ => reactiveValidatable.ReactiveValidator.Validate ( reactiveValidatable, CancellationToken.None, properties ) );
        }

        public static IDisposable ValidateWhen < T, TSignal > ( this T reactiveValidatable, IObservable < TSignal > signal, IValidatorSelector? selector = null, string? ruleSet = null ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return signal.Subscribe ( _ => reactiveValidatable.ReactiveValidator.Validate ( reactiveValidatable, CancellationToken.None, selector, ruleSet ) );
        }

        public static IDisposable ValidateOnPropertyChanged < T > ( this T reactiveValidatable, IObservable < string > propertyChanged ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return propertyChanged.Subscribe ( property => reactiveValidatable.ReactiveValidator.Validate ( reactiveValidatable, CancellationToken.None, property ) );
        }

        public static IDisposable ValidateOnPropertyChanged < T > ( this T reactiveValidatable, IObservable < string > propertyChanged, TimeSpan throttle ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ValidateOnPropertyChanged ( propertyChanged.GroupByUntil ( _ => true, _ => Observable.Timer ( throttle ) )
                                                                                  .SelectMany   ( buffer => buffer.Distinct ( ).ToList ( ) ) );
        }

        public static IDisposable ValidateOnPropertyChanged < T > ( this T reactiveValidatable, IObservable < IEnumerable < string > > propertiesChanged ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return propertiesChanged.Subscribe ( properties => reactiveValidatable.ReactiveValidator.Validate ( reactiveValidatable, CancellationToken.None, properties.ToArray ( ) ) );
        }

        public static IDisposable ValidateOnPropertyChanged < T > ( this T reactiveValidatable, IObservable < IEnumerable < string > > propertiesChanged, TimeSpan throttle ) where T : class, IReactiveValidatable < T >
        {
            if ( reactiveValidatable == null )
                throw new ArgumentNullException ( nameof ( reactiveValidatable ) );

            return reactiveValidatable.ValidateOnPropertyChanged ( propertiesChanged.SelectMany   ( _ => _ )
                                                                                    .GroupByUntil ( _ => true, _ => Observable.Timer ( throttle ) )
                                                                                    .SelectMany   ( buffer => buffer.Distinct ( ).ToList ( ) ) );
        }
    }
}