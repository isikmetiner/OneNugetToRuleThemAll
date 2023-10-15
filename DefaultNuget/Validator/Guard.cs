namespace DefaultNuget.Validator
{
    public sealed class ValidatedNotNullAttribute : Attribute { }

    public static class Guard
    {
        public static void NotNull<T>([ValidatedNotNull] T value, string name) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}