namespace CurrencyConverter.DTOs
{
    public class LatestRateResponseDto
    {
        public string Base { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
