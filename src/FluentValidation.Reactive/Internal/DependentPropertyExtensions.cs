using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using FluentValidation.Internal;

namespace FluentValidation.Reactive.Internal
{
    internal static class DependentPropertyExtensions
    {
        private const string DependenciesKey = "_FVR_Dependencies";

        public static void AddDependencies ( this IValidationContext context, string property, IEnumerable < LambdaExpression > expressions )
        {
            if ( ! ( context.RootContextData.TryGetValue ( DependenciesKey, out var value ) && value is Dictionary < string, HashSet < string > > dependencies ) )
                context.RootContextData [ DependenciesKey ] = dependencies = new Dictionary < string, HashSet < string > > ( );

            if ( ! dependencies.TryGetValue ( property, out var dependents ) )
                dependencies [ property ] = dependents = new HashSet < string > ( );

            foreach ( var expression in expressions )
                dependents.Add ( context.PropertyChain.BuildPropertyName ( PropertyChain.FromExpression ( expression ).ToString ( ) ) );
        }

        public static IEnumerable < string > GetDependencies ( this IValidationContext context, string property )
        {
            if ( ! ( context.RootContextData.TryGetValue ( DependenciesKey, out var value ) && value is Dictionary < string, HashSet < string > > dependencies ) )
                return Enumerable.Empty < string > ( );

            if ( dependencies.TryGetValue ( property, out var dependents ) )
                return dependents;

            return Enumerable.Empty < string > ( );
        }

        public static IEnumerable < string > GetDependencies ( this IValidationRule rule, IValidationContext context )
        {
            return rule.Validators.OfType < DependentPropertyValidator > ( )
                       .SelectMany ( validator  => validator.Dependencies.Select ( PropertyChain.FromExpression ) )
                       .Select     ( dependency => context.PropertyChain.BuildPropertyName ( dependency.ToString ( ) ) );
        }
    }
}