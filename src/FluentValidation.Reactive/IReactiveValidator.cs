using System;
using System.Threading;

using FluentValidation.Results;

namespace FluentValidation.Reactive
{
    public interface IReactiveValidator < T > where T : class
    {
        IValidator  < T >                Validator        { get; }
        IObservable < ValidationResult > ValidationResult { get; }

        void Validate ( ValidationContext < T > context, CancellationToken cancellationToken = default );
        void Reset    (                                  CancellationToken cancellationToken = default );
    }
}