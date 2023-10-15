namespace DefaultNuget.Dto
{
    public class ApiRequest
    {
        public object Request { get; set; }
        public string Controller {  get; set; }
        public string Endpoint { get; set; }
        public string MethodName { get; set; }
        public string Prefix { get; set; }
        public HttpMethod MethodType { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public bool ForceHttps { get; set; }
        public bool UseContext {  get; set; }
    }
}