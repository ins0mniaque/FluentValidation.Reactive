using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation.Results;
using FluentValidation.Validators;

namespace FluentValidation.Reactive.Internal
{
    public class DependentPropertyValidator : IPropertyValidator
    {
        public DependentPropertyValidator ( IEnumerable < LambdaExpression > dependencies )
        {
            Dependencies = new ReadOnlyCollection < LambdaExpression > ( dependencies.ToList ( ) );
            Options      = new PropertyValidatorOptions ( );
        }

        public IReadOnlyCollection < LambdaExpression > Dependencies { get; }
        public PropertyValidatorOptions                 Options      { get; }

        public bool ShouldValidateAsynchronously ( IValidationContext context ) => true;

        public IEnumerable < ValidationFailure >          Validate      ( PropertyValidatorContext context )                                 => Enumerable.Empty < ValidationFailure > ( );
        public Task < IEnumerable < ValidationFailure > > ValidateAsync ( PropertyValidatorContext context, CancellationToken cancellation ) => Task.FromResult ( Enumerable.Empty < ValidationFailure > ( ) );
    }
}