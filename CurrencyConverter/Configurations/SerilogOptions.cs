namespace CurrencyConverter.Configurations
{
    public class SerilogOptions
    {
        public MinimumLevelOptions MinimumLevel { get; set; } = new();
        public List<string> Enrich { get; set; } = new();
        public List<WriteToOptions> WriteTo { get; set; } = new();
    }

    public class MinimumLevelOptions
    {
        public string Default { get; set; } = "Information";
        public Dictionary<string, string> Override { get; set; } = new();
    }

    public class WriteToOptions
    {
        public string Name { get; set; } = string.Empty;
        public WriteToArgs? Args { get; set; }
    }

    public class WriteToArgs
    {
        public string Path { get; set; } = string.Empty;
        public string RollingInterval { get; set; } = "Day";
    }

}
