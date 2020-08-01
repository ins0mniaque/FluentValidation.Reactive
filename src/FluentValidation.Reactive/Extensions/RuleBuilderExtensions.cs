using System;
using System.Linq;
using System.Linq.Expressions;

using FluentValidation.Reactive.Internal;

namespace FluentValidation.Reactive
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions < T, TProperty > DependsOn < T, TProperty > ( this IRuleBuilderOptions < T, TProperty > rule, params Expression < Func < T, object? > > [ ] expressions )
        {
            if ( rule               == null ) throw new ArgumentNullException       ( nameof ( rule        ) );
            if ( expressions        == null ) throw new ArgumentNullException       ( nameof ( expressions ) );
            if ( expressions.Length == 0    ) throw new ArgumentOutOfRangeException ( nameof ( expressions ) );

            var dependencyValidator = new DependentPropertyValidator ( expressions.Select ( Uncast ) );

            rule.Configure ( config =>
            {
                config.ReplaceValidator ( config.CurrentValidator, new PassthroughValidator < T > ( config.CurrentValidator, context =>
                {
                    context.ParentContext.AddDependencies ( context.PropertyName, dependencyValidator.Dependencies );
                } ) );

                var currentValidator = config.CurrentValidator;

                config.RemoveValidator ( currentValidator    );
                config.AddValidator    ( dependencyValidator );
                config.AddValidator    ( currentValidator    );
            } );

            return rule;
        }

        private static LambdaExpression Uncast ( LambdaExpression expression )
        {
            if ( expression.Body is UnaryExpression unary && ( unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked ) )
                return Expression.Lambda ( unary.Operand, expression.Parameters );

            return expression;
        }
    }
}