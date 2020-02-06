using System;

using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public interface IReactiveValidator < T > where T : class
    {
        IValidator  < T >                Validator        { get; }
        IObservable < ValidationResult > ValidationResult { get; }

        T? Instance { get; set; }

        void Validate ( );
    }
}