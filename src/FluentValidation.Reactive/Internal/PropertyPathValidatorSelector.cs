using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using FluentValidation.Internal;
using FluentValidation.Reactive.Validators;

namespace FluentValidation.Reactive.Internal
{
    /// <summary>
    /// Selects validators that are associated with a particular property path.
    /// </summary>
    public class PropertyPathValidatorSelector : IValidatorSelector
    {
        private static readonly Lazy < Regex > IndexerMatcher = new Lazy < Regex > ( ( ) => new Regex ( @"\[\d\]", RegexOptions.Compiled ) );

        private const string DisableCascadeKey = "_FV_DisableSelectorCascadeForChildRules";

        /// <summary>
        /// Creates a new instance of PropertyPathValidatorSelector.
        /// </summary>
        public PropertyPathValidatorSelector ( IEnumerable < string > propertyPaths )
        {
            PropertyPaths = propertyPaths;
            HasIndexers   = propertyPaths.Any ( path => path.Contains ( "[]" ) );
        }

        /// <summary>
        /// Property paths that are validated.
        /// </summary>
        public IEnumerable < string > PropertyPaths { get; }

        /// <summary>
        /// Indicates if validated property paths contain indexers.
        /// </summary>
        public bool HasIndexers { get; }

        /// <summary>
        /// Determines whether or not a rule should execute.
        /// </summary>
        /// <param name="rule">The rule</param>
        /// <param name="propertyPath">Property path (eg Customer.Address.Line1)</param>
        /// <param name="context">Contextual information</param>
        /// <returns>Whether or not the validator can execute.</returns>
        public bool CanExecute ( IValidationRule rule, string propertyPath, IValidationContext context )
        {
            if ( context == null )
                throw new ArgumentNullException ( nameof ( context ) );

            if ( propertyPath == null )
                throw new ArgumentNullException ( nameof ( propertyPath ) );

            if ( HasIndexers && propertyPath.IndexOf ( '[' ) >= 0 )
                propertyPath = IndexerMatcher.Value.Replace ( propertyPath, "[]" );

            var dependencies = Enumerable.Empty < string > ( );
            if ( rule != null && rule.Validators.Any ( validator => validator is DependencyPropertyValidator ) )
            {
                dependencies = rule.Validators.OfType < DependencyPropertyValidator > ( )
                                              .SelectMany ( validator  => validator.Dependencies.Select ( PropertyChain.FromExpression ) )
                                              .Select     ( dependency => context.PropertyChain.BuildPropertyName ( dependency.ToString ( ) ) );

                if ( HasIndexers )
                    dependencies = dependencies.Select ( propertyPath => IndexerMatcher.Value.Replace ( propertyPath, "[]" ) );
            }

            // Validator selector only applies to the top level.
            // If we're running in a child context then this means that the child validator has already been selected
            // Because of this, we assume that the rule should continue (ie if the parent rule is valid, all children are valid)
            var isChildContext = context.IsChildContext;
            var cascadeEnabled = ! context.RootContextData.ContainsKey ( DisableCascadeKey );

            return isChildContext && cascadeEnabled && ! PropertyPaths.Any ( x => x.Contains ( '.' ) ) ||
                   rule is IIncludeRule ||
                   Matches ( propertyPath ) ||
                   dependencies.Any ( Matches );

            bool Matches ( string propertyPath ) => PropertyPaths.Any ( path => path == propertyPath ||
                                                                                IsSubPath ( propertyPath, path ) ||
                                                                                IsSubPath ( path, propertyPath ) );
        }

        private static bool IsSubPath ( string path, string parentPath )
        {
            return path.Length > parentPath.Length &&
                   ( path [ parentPath.Length ] == '.' ||
                     path [ parentPath.Length ] == '[' ) &&
                   path.StartsWith ( parentPath, StringComparison.Ordinal );
        }
    }
}