using System;
using System.Linq;
using System.Linq.Expressions;

using FluentValidation.Reactive.Internal;
using FluentValidation.Reactive.Validators;

namespace FluentValidation.Reactive
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions < T, TProperty > DependsOn < T, TProperty > ( this IRuleBuilderOptions < T, TProperty > ruleBuilderOptions, params Expression < Func < T, object? > > [ ] expressions )
        {
            if ( ruleBuilderOptions == null ) throw new ArgumentNullException       ( nameof ( ruleBuilderOptions ) );
            if ( expressions        == null ) throw new ArgumentNullException       ( nameof ( expressions ) );
            if ( expressions.Length == 0    ) throw new ArgumentOutOfRangeException ( nameof ( expressions ) );

            var validator = new DependencyPropertyValidator ( expressions.Select ( Uncast ) );

            ruleBuilderOptions.OnFailure ( (_, context) => context.ParentContext.AddDependencies ( context.PropertyName, validator.Dependencies ) );
            ruleBuilderOptions.Configure ( config =>
            {
                var currentValidator = config.CurrentValidator;
                if ( currentValidator != null )
                    config.RemoveValidator ( currentValidator );

                config.AddValidator ( validator );

                if ( currentValidator != null )
                    config.AddValidator ( currentValidator );
            } );

            return ruleBuilderOptions;
        }

        private static LambdaExpression Uncast ( LambdaExpression expression )
        {
            if ( expression.Body is UnaryExpression unary && ( unary.NodeType == ExpressionType.Convert || unary.NodeType == ExpressionType.ConvertChecked ) )
                return Expression.Lambda ( unary.Operand, expression.Parameters );

            return expression;
        }
    }
}