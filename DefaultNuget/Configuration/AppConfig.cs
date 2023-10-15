using DefaultNuget.Enum;

namespace DefaultNuget.Configuration
{
    public class AppConfig
    {
        public Dictionary<string, string> UrlMapping { get; set; }
        public LogType LogType { get; set; }
        public string Environment {  get; set; }
        public string ApplicationName { get; set; }
    }
}