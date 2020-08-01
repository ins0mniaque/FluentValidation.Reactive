using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentValidation.Results;
using FluentValidation.Validators;

namespace FluentValidation.Reactive.Internal
{
    public class PassthroughValidator < T > : IPropertyValidator, IChildValidatorAdaptor
    {
        private readonly IPropertyValidator                  validator;
        private readonly Action < PropertyValidatorContext > onFailure;

        public PassthroughValidator ( IPropertyValidator validator, Action < PropertyValidatorContext > onFailure )
        {
            this.validator = validator;
            this.onFailure = onFailure;
        }

        public PropertyValidatorOptions Options       => validator.Options;
        public Type                     ValidatorType => validator is IChildValidatorAdaptor adaptor ? adaptor.ValidatorType : validator.GetType ( );

        public bool ShouldValidateAsynchronously ( IValidationContext context ) => validator.ShouldValidateAsynchronously ( context );

        public IEnumerable < ValidationFailure > Validate ( PropertyValidatorContext context )
        {
            var list = validator.Validate ( context ).ToList ( );
            if ( list.Any ( ) )
                onFailure ( context );

            return list;
        }

        public async Task < IEnumerable < ValidationFailure > > ValidateAsync ( PropertyValidatorContext context, CancellationToken cancellation )
        {
            var list = ( await validator.ValidateAsync ( context, cancellation ).ConfigureAwait ( false ) ).ToList ( );
            if ( list.Any ( ) )
                onFailure ( context );

            return list;
        }
    }
}