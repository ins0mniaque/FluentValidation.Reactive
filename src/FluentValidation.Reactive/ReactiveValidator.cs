using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public class ReactiveValidator < T > : IReactiveValidator < T > where T : class
    {
        private ISubject < Unit > signal = new Subject < Unit > ( );

        public ReactiveValidator ( IValidator < T > validator )
        {
            if ( validator == null )
                throw new ArgumentNullException ( nameof ( validator ) );

            Validator        = validator;
            ValidationResult = signal.Select   ( _ => ObservableValidate ( ) )
                                     .Switch   ( )
                                     .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance )
                                     .Replay   ( 1 )
                                     .RefCount ( );
        }

        public IValidator  < T >                Validator        { get; }
        public IObservable < ValidationResult > ValidationResult { get; }

        public T? Instance { get; set; }

        public void Validate ( ) => signal.OnNext ( Unit.Default );

        private IObservable < ValidationResult > ObservableValidate ( )
        {
            var instance = Instance;
            if ( instance != null )
                return Observable.FromAsync ( cancellationToken => Validator.ValidateAsync ( instance, cancellationToken ) );

            return Observable.Empty < ValidationResult > ( );
        }
    }
}