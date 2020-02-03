using System;

namespace FluentValidation.Reactive
{
    public class ReflectionBasedReactiveValidator < T > : ReactiveValidator < T > where T : class
    {
        public ReflectionBasedReactiveValidator ( IValidatorFactory validatorFactory ) : base ( GetValidator ( validatorFactory ) ) { }

        private static IValidator < T > GetValidator ( IValidatorFactory validatorFactory )
        {
            if ( validatorFactory == null )
                throw new ArgumentNullException ( nameof ( validatorFactory ) );

            return validatorFactory.GetValidator < T > ( ) ??
                   new ReflectionBasedValidator  < T > ( validatorFactory );
        }
    }
}