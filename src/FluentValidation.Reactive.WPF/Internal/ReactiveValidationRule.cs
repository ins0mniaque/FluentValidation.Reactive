using System.Globalization;
using System.Windows.Controls;

namespace FluentValidation.Reactive.Internal
{
    /// <summary>
    /// Represents a rule that checks for errors that are raised by FluentValidation.Reactive.
    /// </summary>
    public sealed class ReactiveValidationRule : ValidationRule
    {
        public static ReactiveValidationRule Instance { get; } = new ReactiveValidationRule ( );

        private ReactiveValidationRule ( ) : base ( ValidationStep.UpdatedValue, true ) { }

        /// <summary>
        /// Performs validation checks on a value.
        /// </summary>
        /// <param name="value">The value (from the binding target) to check.</param>
        /// <param name="cultureInfo">The culture to use in this rule.</param>
        /// <returns>A <see cref="ValidationResult" /> object.</returns>
        public override ValidationResult Validate ( object value, CultureInfo cultureInfo ) => ValidationResult.ValidResult;
    }
}