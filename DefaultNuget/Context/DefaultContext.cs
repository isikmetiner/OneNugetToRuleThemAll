namespace DefaultNuget.Context
{
    public class DefaultContext
    {
        private static readonly AsyncLocal<DefaultContext> current = new();

        public static DefaultContext Current
        {
            get
            {
                return current.Value ??= new DefaultContext();
            }
            set 
            {
                current.Value = value;
            }
        }
    }
}