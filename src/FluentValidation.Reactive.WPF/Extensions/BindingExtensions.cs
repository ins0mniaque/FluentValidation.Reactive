using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;

using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public static class BindingExtensions
    {
        public static IDisposable Bind ( this IObservable < ValidationResult > validationResult, DependencyObject targetElement )
        {
            if ( targetElement == null )
                throw new ArgumentNullException ( nameof ( targetElement ) );

            return validationResult.ObserveOn ( new DispatcherSynchronizationContext ( targetElement.Dispatcher ) )
                                   .Bind      ( Internal.Validation.ToValidationError,
                                                error => Internal.Validation.AddValidationError    ( error, targetElement, true ),
                                                error => Internal.Validation.RemoveValidationError ( error, targetElement, true ) );
        }
    }
}