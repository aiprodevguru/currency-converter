namespace CurrencyConverter.ViewModels
{
    public class RateViewModel
    {
        public DateOnly Date { get; set; }
        public required Dictionary<string, decimal> Rate { get; set; }
    }

}
