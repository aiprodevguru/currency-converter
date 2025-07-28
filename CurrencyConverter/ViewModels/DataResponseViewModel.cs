namespace CurrencyConverter.ViewModels
{
    public class DataResponseViewModel<T>
    {
        public bool Success { get; set; } = true;
        public required string Message { get; set; }
        public required T Data { get; set; }
    }
}
