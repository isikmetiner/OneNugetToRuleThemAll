namespace DefaultNuget.Dto
{
    public class ApiResponse
    {
        public bool IsSuccessful { get; set; }
        public string Message { get; set; }
        public object Response { get; set; }
        public T CastGeneric<T>()
        {
            return (T)Response;
        }
    }
}