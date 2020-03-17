namespace FluentValidation.Reactive
{
    public static class ReactiveValidatorOptions
    {
        public static void EnablePropertyPathIndexerSupport ( )
        {
            ValidatorOptions.ValidatorSelectors.MemberNameValidatorSelectorFactory = memberNames => new Internal.PropertyPathValidatorSelector ( memberNames );
        }
    }
}