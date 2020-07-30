using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation.Results;
using FluentValidation.Validators;

namespace FluentValidation.Reactive.Internal
{
    public static class ValidationResultDiffTool
    {
        public static ValidationResult Merge ( IValidationContext? previousContext, ValidationResult previousResult, IValidationContext? currentContext, ValidationResult currentResult )
        {
            if ( previousResult == null ) throw new ArgumentNullException ( nameof ( previousResult ) );
            if ( currentResult  == null ) throw new ArgumentNullException ( nameof ( currentResult  ) );

            if ( currentContext != null && ReferenceEquals ( previousContext?.InstanceToValidate, currentContext.InstanceToValidate ) )
            {
                var fakeRule = new FakeRule { RuleSets = previousResult.RuleSetsExecuted };
                var selector = currentContext.Selector;

                bool NotInvalidated ( ValidationFailure error ) => ! selector.CanExecute ( fakeRule, error.PropertyName, previousContext );

                foreach ( var error in previousResult.Errors.Where ( NotInvalidated ) )
                    currentResult.Errors.Add ( error );
            }

            return currentResult;
        }

        private class FakeRule : IValidationRule
        {
            public string [ ]? RuleSets { get; set; }

            public IEnumerable < IPropertyValidator > Validators => throw new NotImplementedException ( );

            public void ApplyCondition      ( Func < PropertyValidatorContext, bool >                             predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators ) => throw new NotImplementedException ( );
            public void ApplyAsyncCondition ( Func < PropertyValidatorContext, CancellationToken, Task < bool > > predicate, ApplyConditionTo applyConditionTo = ApplyConditionTo.AllValidators ) => throw new NotImplementedException ( );

            public void ApplySharedCondition      ( Func < IValidationContext, bool >                             condition ) => throw new NotImplementedException ( );
            public void ApplySharedAsyncCondition ( Func < IValidationContext, CancellationToken, Task < bool > > condition ) => throw new NotImplementedException ( );

            public IEnumerable < ValidationFailure >          Validate      ( IValidationContext context )                                 => throw new NotImplementedException ( );
            public Task < IEnumerable < ValidationFailure > > ValidateAsync ( IValidationContext context, CancellationToken cancellation ) => throw new NotImplementedException ( );
        }
    }
}