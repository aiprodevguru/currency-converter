namespace CurrencyConverter.Configurations
{
    public class OpenTelemetryOptions
    {
        public bool Enabled { get; set; }
        public string Exporter { get; set; } = string.Empty;
        public string OtlpEndpoint { get; set; } = string.Empty;
    }
}
