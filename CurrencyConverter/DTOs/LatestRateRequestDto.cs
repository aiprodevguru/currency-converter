namespace CurrencyConverter.DTOs
{
    public class LatestRateRequestDto
    {
        public required string Base { get; set; }
        public string? Provider { get; set; }
    }
}
