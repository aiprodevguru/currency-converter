namespace CurrencyConverter.ViewModels
{
    public class NoContentResponseViewModel
    {
        public bool Success { get; set; } = true;
        public required string Message { get; set; }
    }
}
