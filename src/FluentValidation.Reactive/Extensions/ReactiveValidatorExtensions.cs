using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;

using FluentValidation.Internal;

namespace FluentValidation.Reactive
{
    public static class ReactiveValidatorExtensions
    {
        /// <summary>
        /// Validates certain properties of the specified instance asynchronously.
        /// </summary>
        /// <param name="validator">The current validator</param>
        /// <param name="instance">The object to validate</param>
        /// <param name="cancellationToken"></param>
        /// <param name="properties">Expressions to specify the properties to validate</param>
        public static void Validate < T > ( this IReactiveValidator < T > validator, T instance, CancellationToken cancellationToken = default, params Expression < Func < T, object > > [ ] properties ) where T : class
        {
            var selector = ValidatorOptions.Global.ValidatorSelectors.MemberNameValidatorSelectorFactory ( MemberNameValidatorSelector.MemberNamesFromExpressions ( properties ) );
            var context  = new ValidationContext < T > ( instance, new PropertyChain ( ), selector );

            validator.Validate ( context, cancellationToken );
        }

        /// <summary>
        /// Validates certain properties of the specified instance asynchronously.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="instance">The object to validate</param>
        /// <param name="cancellationToken"></param>
        /// <param name="properties">The names of the properties to validate.</param>
        public static void Validate < T > ( this IReactiveValidator < T > validator, T instance, CancellationToken cancellationToken = default, params string [ ] properties ) where T : class
        {
            var selector = ValidatorOptions.Global.ValidatorSelectors.MemberNameValidatorSelectorFactory ( properties );
            var context  = new ValidationContext < T > ( instance, new PropertyChain ( ), selector );

            validator.Validate ( context, cancellationToken );
        }

        /// <summary>
        /// Validates an object asynchronously using a custom validator selector or a ruleset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="validator"></param>
        /// <param name="instance"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="selector"></param>
        /// <param name="ruleSet"></param>
        [ SuppressMessage ( "Design",        "CA1068:CancellationToken parameters must come last",  Justification = "Matching FluentValidation IValidator.ValidateAsync extension signature" ) ]
        [ SuppressMessage ( "Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Matching FluentValidation IValidator.ValidateAsync extension exception" ) ]
        public static void Validate < T > ( this IReactiveValidator < T > validator, T instance, CancellationToken cancellationToken = default, IValidatorSelector? selector = null, string? ruleSet = null ) where T : class
        {
            if ( selector != null && ruleSet != null )
                throw new InvalidOperationException ( "Cannot specify both an IValidatorSelector and a RuleSet." );

            selector ??= ValidatorOptions.Global.ValidatorSelectors.DefaultValidatorSelectorFactory ( );

            if ( ruleSet != null )
            {
                var ruleSetNames = ruleSet.Split ( ',', ';' );

                selector = ValidatorOptions.Global.ValidatorSelectors.RulesetValidatorSelectorFactory ( ruleSetNames );
            }

            var context = new ValidationContext < T > ( instance, new PropertyChain ( ), selector );

            validator.Validate ( context, cancellationToken );
        }
    }
}