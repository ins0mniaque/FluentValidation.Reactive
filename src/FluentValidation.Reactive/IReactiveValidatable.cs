namespace FluentValidation.Reactive
{
    public interface IReactiveValidatable < T > where T : class
    {
        IReactiveValidator < T > ReactiveValidator { get; }
    }
}