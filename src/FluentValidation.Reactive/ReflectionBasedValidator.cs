using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

using FluentValidation.Internal;
using FluentValidation.Validators;

namespace FluentValidation.Reactive
{
    using static Expression;

    [ SuppressMessage ( "Naming", "CA1710:Identifiers should have correct suffix" ) ]
    public class ReflectionBasedValidator < T > : AbstractValidator < T >
    {
        public ReflectionBasedValidator ( IValidatorFactory validatorFactory )
        {
            if ( validatorFactory == null )
                throw new ArgumentNullException ( nameof ( validatorFactory ) );

            foreach ( var property in typeof ( T ).GetProperties ( ) )
            {
                if ( ! property.CanRead )
                    continue;

                var validator = validatorFactory.GetValidator ( property.PropertyType );
                if ( validator == null )
                    continue;

                var rule = new PropertyRule ( property,
                                              NonGenericExpression ( property ).Compile ( ),
                                              GenericExpression    ( property ),
                                              ( ) => ValidatorOptions.Global.CascadeMode,
                                              property.PropertyType,
                                              typeof ( T ) );

                var adaptorType = typeof ( ChildValidatorAdaptor < , > ).MakeGenericType ( typeof ( T ), property.PropertyType );

                rule.AddValidator ( (IPropertyValidator) Activator.CreateInstance ( adaptorType, validator, validator.GetType ( ) ) );

                AddRule ( rule );
            }
        }

        private static LambdaExpression GenericExpression ( PropertyInfo property )
        {
            var instance = Parameter ( typeof ( T ) );

            return Lambda ( Property ( instance, property ), instance );
        }

        private static Expression < Func < object, object > > NonGenericExpression ( PropertyInfo property )
        {
            var instance = Parameter ( typeof ( object ) );

            return Lambda < Func < object, object > > ( Convert ( Property ( Convert ( instance, typeof ( T ) ), property ), typeof ( object ) ), instance );
        }
    }
}