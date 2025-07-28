using CurrencyConverter.DTOs;
using CurrencyConverter.ViewModels;
public interface IExchangeRateService
{
    Task<LatestRateResponseDto> GetLatestRatesAsync(LatestRateRequestDto dto);
    Task<ConvertCurrencyResponseDto> ConvertCurrencyAsync(ConvertCurrencyRequestDto dto);
    Task<HistoricalRatesViewModel> GetHistoricalRatesAsync(HistoricalRatesRequestDto dto);
}


