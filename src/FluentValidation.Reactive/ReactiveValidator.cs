using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation.Results;
using FluentValidation.Validators;

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
                                     .Select   ( window => MergeResults ( window.Previous, window.Current ) )
                                     .DistinctUntilChanged ( Internal.ValidationResultEqualityComparer.Instance )
                                     .Replay   ( 1 )
                                     .RefCount ( );
        }

        public IValidator  < T >                Validator        { get; }
        public IObservable < ValidationResult > ValidationResult { get; }

        public void Validate ( ValidationContext < T > context, CancellationToken cancellationToken = default )
        {
            signal.OnNext ( new ValidationRequest ( context, cancellationToken ) );
        }

        public void Reset ( CancellationToken cancellationToken = default )
        {
            signal.OnNext ( new ValidationRequest ( null, cancellationToken ) );
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

        private ValidationResult MergeResults ( ContextualValidationResult previous, ContextualValidationResult current )
        {
            if ( current.Context != null && ReferenceEquals ( previous.Context?.InstanceToValidate, current.Context.InstanceToValidate ) )
            {
                var fakeRule = new FakeRule { RuleSets = previous.Result.RuleSetsExecuted };
                var selector = current.Context.Selector;

                bool NotInvalidated ( ValidationFailure error ) => ! selector.CanExecute ( fakeRule, error.PropertyName, previous.Context );

                foreach ( var error in previous.Result.Errors.Where ( NotInvalidated ) )
                    current.Result.Errors.Add ( error );
            }

            return current.Result;
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

        private class FakeRule : IValidationRule
        {
            public string [ ]? RuleSets { get; set; }

            public IEnumerable < IPropertyValidator > Validators => throw new NotImplementedException ( );

            public void ApplyCondition      ( Func < PropertyValidatorContext, bool >                             predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators ) => throw new NotImplementedException ( );
            public void ApplyAsyncCondition ( Func < PropertyValidatorContext, CancellationToken, Task < bool > > predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators ) => throw new NotImplementedException ( );

            public IEnumerable < ValidationFailure >          Validate      ( ValidationContext context )                                 => throw new NotImplementedException ( );
            public Task < IEnumerable < ValidationFailure > > ValidateAsync ( ValidationContext context, CancellationToken cancellation ) => throw new NotImplementedException ( );
        }
    }
}