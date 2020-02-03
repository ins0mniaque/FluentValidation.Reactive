using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using FluentValidation.Results;

namespace FluentValidation.Reactive.Internal
{
    public static class Validation
    {
        public static void AddValidationError ( ValidationError validationError, DependencyObject targetElement, bool shouldRaiseEvent )
        {
            AddValidationErrorMethod.Invoke ( null, new object [ ] { validationError, targetElement, shouldRaiseEvent } );
        }

        public static void RemoveValidationError ( ValidationError validationError, DependencyObject targetElement, bool shouldRaiseEvent )
        {
            RemoveValidationErrorMethod.Invoke ( null, new object [ ] { validationError, targetElement, shouldRaiseEvent } );
        }

        public static ValidationError ToValidationError ( this ValidationFailure error )
        {
            if ( error == null )
                throw new ArgumentNullException ( nameof ( error ) );

            return new ValidationError ( ReactiveValidationRule.Instance,
                                         error.PropertyName,
                                         error,
                                         null );
        }

        private static readonly MethodInfo AddValidationErrorMethod    = GetValidationMethod ( nameof ( AddValidationError    ) );
        private static readonly MethodInfo RemoveValidationErrorMethod = GetValidationMethod ( nameof ( RemoveValidationError ) );

        private static MethodInfo GetValidationMethod ( string methodName )
        {
            var validationType = typeof ( System.Windows.Controls.Validation );

            return validationType.GetMethod ( methodName, BindingFlags.NonPublic | BindingFlags.Static ) ??
                   throw new MissingMethodException ( validationType.Name, methodName );
        }
    }
}