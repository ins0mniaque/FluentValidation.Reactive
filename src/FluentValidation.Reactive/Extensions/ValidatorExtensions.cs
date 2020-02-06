using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;

using FluentValidation.Internal;
using FluentValidation.Validators;

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
            var parameter    = expression.Parameters [ 0 ].ToString ( ) + ".";
            var propertyPath = expression.Body.ToString ( ).Replace ( ".Item[0]", "[]" );
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

                var childValidators = rule.Validators
                                          .OfType < ChildValidatorAdaptor > ( )
                                          .Select ( adaptor => adaptor.GetValidator ( emptyContext ) );

                foreach ( var childValidator in childValidators )
                    foreach ( var childExpression in childValidator.GetValidatedExpressions ( expression ) )
                        yield return childExpression;
            }
        }

        private static bool IsCollectionPropertyRuleType ( Type type )
        {
            while ( type != null )
            {
                if ( type.IsGenericType && type.GetGenericTypeDefinition ( ) == typeof ( CollectionPropertyRule < > ) )
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        private static LambdaExpression AddIndexer ( LambdaExpression expression )
        {
            var indexer = expression.Body.Type.GetProperty ( "Item" );
            if ( indexer == null )
                throw new NotSupportedException ( $"Collection type { expression.Body.Type.Name } has no indexer" );

            return Expression.Lambda ( Expression.MakeIndex ( expression.Body,
                                                              indexer,
                                                              new [ ] { Expression.Constant ( 0 ) } ),
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