using System;
using System.Windows;

using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public static class BindingExtensions
    {
        public static IDisposable Bind ( this IObservable < ValidationResult > validationResult, DependencyObject targetElement )
        {
            return validationResult.Subscribe ( Internal.Validation.ToValidationError,
                                                error => Internal.Validation.AddValidationError    ( error, targetElement, true ),
                                                error => Internal.Validation.RemoveValidationError ( error, targetElement, true ) );
        }
    }
}