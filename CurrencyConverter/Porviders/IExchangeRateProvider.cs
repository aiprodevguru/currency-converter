using CurrencyConverter.DTOs;
using CurrencyConverter.ViewModels;

namespace CurrencyConverter.Porviders
{
    public interface IExchangeRateProvider
    {
        public Task<LatestRateResponseDto> GetLatestRatesAsync(string baseCurrency);
        public Task<ConvertCurrencyResponseDto> ConvertCurrencyAsync(string from, string to, decimal amount);
        public Task<HistoricalRatesViewModel> GetHistoricalRatesAsync(string baseCurrency, DateOnly start, DateOnly end, int page, int pageSize);
    }
}
