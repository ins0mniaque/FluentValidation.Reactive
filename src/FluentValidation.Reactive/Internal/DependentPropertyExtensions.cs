using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using FluentValidation.Internal;

namespace FluentValidation.Reactive.Internal
{
    public static class DependentPropertyExtensions
    {
        private const string DependenciesKey = "_FVR_Dependencies";

        public static void AddDependencies ( this IValidationContext context, string property, IEnumerable < LambdaExpression > expressions )
        {
            if ( context     == null ) throw new ArgumentNullException ( nameof ( context     ) );
            if ( property    == null ) throw new ArgumentNullException ( nameof ( property    ) );
            if ( expressions == null ) throw new ArgumentNullException ( nameof ( expressions ) );

            if ( ! ( context.RootContextData.TryGetValue ( DependenciesKey, out var value ) && value is Dictionary < string, HashSet < string > > dependencies ) )
                context.RootContextData [ DependenciesKey ] = dependencies = new Dictionary < string, HashSet < string > > ( );

            if ( ! dependencies.TryGetValue ( property, out var dependents ) )
                dependencies [ property ] = dependents = new HashSet < string > ( );

            foreach ( var expression in expressions )
                dependents.Add ( context.PropertyChain.BuildPropertyName ( PropertyChain.FromExpression ( expression ).ToString ( ) ) );
        }

        public static IEnumerable < string > GetDependencies ( this IValidationContext context, string property )
        {
            if ( context  == null ) throw new ArgumentNullException ( nameof ( context  ) );
            if ( property == null ) throw new ArgumentNullException ( nameof ( property ) );

            if ( ! ( context.RootContextData.TryGetValue ( DependenciesKey, out var value ) && value is Dictionary < string, HashSet < string > > dependencies ) )
                return Enumerable.Empty < string > ( );

            if ( dependencies.TryGetValue ( property, out var dependents ) )
                return dependents;

            return Enumerable.Empty < string > ( );
        }

        public static IEnumerable < string > GetDependencies ( this IValidationRule rule, IValidationContext context )
        {
            if ( rule    == null ) throw new ArgumentNullException ( nameof ( rule    ) );
            if ( context == null ) throw new ArgumentNullException ( nameof ( context ) );

            return rule.Validators.OfType < DependentPropertyValidator > ( )
                       .SelectMany ( validator  => validator.Dependencies.Select ( PropertyChain.FromExpression ) )
                       .Select     ( dependency => context.PropertyChain.BuildPropertyName ( dependency.ToString ( ) ) );
        }
    }
}