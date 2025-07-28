namespace CurrencyConverter.DTOs
{
    public class HistoricalRatesresponseDto
    {
        public required DateOnly Date {  get; set; }
        public required Dictionary<string, decimal> Rates { get; set; }
    }
}
