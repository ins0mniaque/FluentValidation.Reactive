using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public class ReactiveValidator < T > : IReactiveValidator < T > where T : class
    {
        private ISubject < ValidationRequest > signal = new Subject < ValidationRequest > ( );

        public ReactiveValidator ( IValidator < T > validator )
        {
            if ( validator == null )
                throw new ArgumentNullException ( nameof ( validator ) );

            var empty = new ContextualValidationResult ( );

            Validator        = validator;
            ValidationResult = signal.Select   ( ValidateAsync )
                                     .Switch   ( )
                                     .Scan     ( (Previous: empty, Current: empty), (window, latest) => (window.Current, latest) )
                                     .Select   ( window => Merge ( window.Previous.Context,
                                                                   window.Previous.Result,
                                                                   window.Current .Context,
                                                                   window.Current .Result ) )
                                     .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance )
                                     .Replay   ( 1 )
                                     .RefCount ( );
        }

        public IValidator  < T >                Validator        { get; }
        public IObservable < ValidationResult > ValidationResult { get; }

        public virtual void Validate ( ValidationContext < T > context, CancellationToken cancellationToken = default )
        {
            signal.OnNext ( new ValidationRequest ( context, cancellationToken ) );
        }

        public virtual void Reset ( CancellationToken cancellationToken = default )
        {
            signal.OnNext ( new ValidationRequest ( null, cancellationToken ) );
        }

        protected virtual ValidationResult Merge ( ValidationContext? previousContext, ValidationResult previousResult, ValidationContext? currentContext, ValidationResult currentResult )
        {
            return Internal.ValidationResultDiffTool.Merge ( previousContext, previousResult, currentContext, currentResult );
        }

        private IObservable < ContextualValidationResult > ValidateAsync ( ValidationRequest request )
        {
            if ( request.Context == null )
                return Observable.Return ( new ContextualValidationResult ( ) );

            return Observable.FromAsync ( ValidateAsync )
                             .Select    ( validationResult => new ContextualValidationResult ( request.Context, validationResult ) );

            Task < ValidationResult > ValidateAsync ( CancellationToken cancellationToken )
            {
                using ( var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource ( request.CancellationToken, cancellationToken ) )
                    return Validator.ValidateAsync ( request.Context, linkedCancellation.Token );
            }
        }

        private class ValidationRequest
        {
            public ValidationRequest ( ValidationContext < T >? context, CancellationToken cancellationToken )
            {
                Context            = context;
                CancellationToken  = cancellationToken;
            }

            public ValidationContext < T >? Context           { get; }
            public CancellationToken        CancellationToken { get; }
        }

        private class ContextualValidationResult
        {
            public ContextualValidationResult ( ) : this ( null, new ValidationResult ( ) ) { }
            public ContextualValidationResult ( ValidationContext < T >? context, ValidationResult result )
            {
                Context = context;
                Result  = result;
            }

            public ValidationContext < T >? Context { get; }
            public ValidationResult         Result  { get; }
        }
    }
}