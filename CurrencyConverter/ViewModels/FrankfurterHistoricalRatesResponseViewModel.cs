namespace CurrencyConverter.ViewModels
{
    public class FrankfurterHistoricalRatesResponseViewModel
    {
        public required decimal Amount { get; set; }
        public required string Base { get; set; }
        public required DateOnly Start_Date { get; set; }
        public required DateOnly End_Date { get; set; }
        public required Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
    }
}
