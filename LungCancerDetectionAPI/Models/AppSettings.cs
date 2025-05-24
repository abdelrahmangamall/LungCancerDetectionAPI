namespace Models
{
    public class AppSettings
    {
        public AIModelSettings AIModelSettings { get; set; }
    }

    public class AIModelSettings
    {
        public string ModelPath { get; set; }
        public string TempImageStoragePath { get; set; }
        public int MaxFileSizeMB { get; set; }
        public string[] AllowedExtensions { get; set; }
    }
}
