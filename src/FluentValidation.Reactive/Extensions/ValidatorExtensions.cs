using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

using FluentValidation.Internal;
using FluentValidation.Validators;
using FluentValidation.Reactive.Validators;

namespace FluentValidation.Reactive
{
    public static class ValidatorExtensions
    {
        private static readonly PropertyValidatorContext emptyContext = new PropertyValidatorContext ( null, null, null );

        public static IEnumerable < string > GetValidatedPropertyPaths ( this IValidator validator )
        {
            return validator.GetValidatedExpressions ( null )
                            .Select   ( GetPropertyPath )
                            .Distinct ( );
        }

        private static string GetPropertyPath ( this LambdaExpression expression )
        {
            var propertyPath = expression.Body.ToString ( ).Replace ( ".Item[0]", "[]" )
                                                           .Replace ( ".First()", "[]" );

            var parameter = expression.Parameters [ 0 ].ToString ( ) + ".";
            if ( propertyPath.StartsWith ( parameter, StringComparison.Ordinal ) )
                propertyPath = propertyPath.Remove ( 0, parameter.Length );

            return propertyPath;
        }

        public static IEnumerable < LambdaExpression > GetValidatedExpressions ( this IValidator validator )
        {
            return validator.GetValidatedExpressions ( null )
                            .GroupBy ( expression => expression.Body.ToString ( ) )
                            .Select  ( expression => expression.First ( ) );
        }

        private static IEnumerable < LambdaExpression > GetValidatedExpressions ( this IValidator validator, LambdaExpression? parent )
        {
            var rules = validator as IEnumerable < IValidationRule >;
            if ( rules == null )
            {
                // TODO: Read rules through descriptor
                throw new NotSupportedException ( $"Validator must implement { typeof ( IEnumerable < IValidationRule > ).Name }" );
            }

            foreach ( var rule in rules.OfType < PropertyRule > ( ) )
            {
                var expression = rule.Expression;

                if ( IsCollectionPropertyRuleType ( rule.GetType ( ) ) )
                    expression = AddIndexer ( expression );

                if ( parent != null && expression != null )
                    expression = Compose ( parent, expression );

                if ( expression != null )
                    yield return expression;

                var dependencies = rule.Validators
                                       .OfType < DependencyPropertyValidator > ( )
                                       .SelectMany ( validator => validator.Dependencies );

                if ( parent != null )
                    dependencies = dependencies.Select ( expression => Compose ( parent, expression ) );

                foreach ( var dependency in dependencies )
                    yield return dependency;

                var childValidators = rule.Validators
                                          .OfType < IChildValidatorAdaptor > ( )
                                          .Select ( adaptor => GetValidator ( adaptor, emptyContext ) );

                foreach ( var childValidator in childValidators )
                    foreach ( var childExpression in childValidator.GetValidatedExpressions ( expression ) )
                        yield return childExpression;
            }
        }

        private static readonly Lazy < Dictionary < Type, MethodInfo > > typedChildValidatorAdaptorCache = new Lazy < Dictionary < Type, MethodInfo > > ( );

        private static IValidator GetValidator ( IChildValidatorAdaptor adaptor, PropertyValidatorContext context )
        {
            var cache       = typedChildValidatorAdaptorCache.Value;
            var adaptorType = adaptor.GetType ( );
            if ( ! cache.TryGetValue ( adaptorType, out var getValidator ) )
                cache [ adaptorType ] = getValidator = adaptorType.GetRuntimeMethod ( nameof ( ChildValidatorAdaptor < object, object >.GetValidator ),
                                                                                      new [ ] { typeof ( PropertyValidatorContext ) } );

            return (IValidator) getValidator.Invoke ( adaptor, new [ ] { context } );
        }

        private static bool IsCollectionPropertyRuleType ( Type type )
        {
            while ( type != null )
            {
                if ( type.IsGenericType && type.GetGenericTypeDefinition ( ) == typeof ( CollectionPropertyRule < , > ) )
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        private static readonly MethodInfo EnumerableFirstMethod = typeof ( Enumerable ).GetMethods ( ).Single ( method => method.Name == nameof ( Enumerable.First ) && method.GetParameters ( ).Length == 1 );

        private static LambdaExpression AddIndexer ( LambdaExpression expression )
        {
            if ( expression.Body.Type.IsArray )
                return Expression.Lambda ( Expression.ArrayIndex ( expression.Body,
                                                                   Expression.Constant ( 0 ) ),
                                           expression.Parameters );

            var indexer = expression.Body.Type.GetProperty ( "Item" );
            if ( indexer != null )
                return Expression.Lambda ( Expression.MakeIndex ( expression.Body,
                                                                  indexer,
                                                                  new [ ] { Expression.Constant ( 0 ) } ),
                                           expression.Parameters );

            var itemType = expression.Body.Type.GetInterfaces ( )
                                               .Where  ( @interface => @interface.IsGenericType )
                                               .Single ( @interface => @interface.GetGenericTypeDefinition ( ) == typeof ( IEnumerable < > ) )
                                               .GetGenericArguments ( ) [ 0 ];

            return Expression.Lambda ( Expression.Call ( null,
                                                         EnumerableFirstMethod.MakeGenericMethod ( itemType ),
                                                         expression.Body ),
                                       expression.Parameters );
        }

        private static LambdaExpression Compose ( LambdaExpression first, LambdaExpression second )
        {
            var body = ExpressionReplacer.Replace ( second.Body,
                                                    second.Parameters [ 0 ],
                                                    first .Body );

            return Expression.Lambda ( body, first.Parameters [ 0 ] );
        }

        private class ExpressionReplacer : ExpressionVisitor
        {
            public static T Replace < T > ( T expression, Expression replace, Expression with ) where T : Expression
            {
                var replacer = new ExpressionReplacer ( replace, with );

                return replacer.VisitAndConvert ( expression, nameof ( Replace ) );
            }

            private readonly Expression replace;
            private readonly Expression with;

            public ExpressionReplacer ( Expression replace, Expression with )
            {
                this.replace = replace;
                this.with    = with;
            }

            public override Expression Visit ( Expression node ) => node == replace ? with : base.Visit ( node );
        }
    }
}