namespace CurrencyConverter.ViewModels
{
    public class HistoricalRatesViewModel
    {
        public required string Base {  get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TotalCount { get; set; }
        public int Page {  get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public decimal Amount { get; set; }
       
        public required IEnumerable<RateViewModel> Data { get; set; }

    }
}
