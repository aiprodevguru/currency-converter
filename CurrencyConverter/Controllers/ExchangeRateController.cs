using CurrencyConverter.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.DTOs;
using CurrencyConverter.ViewModels;
using CurrencyConverter.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ExchangeRateController : BaseApiController
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRateController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }
    
    [HttpGet("Latest")]
    public async Task<ActionResult<DataResponseViewModel<LatestRateResponseDto>>> GetLatestRates([FromQuery] LatestRateRequestDto dto)
    {
        var result = await _exchangeRateService.GetLatestRatesAsync(dto);
        return OkResponse<LatestRateResponseDto>(result);
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("Convert")]
    public async Task<ActionResult<DataResponseViewModel<ConvertCurrencyResponseDto>>> ConvertCurrency([FromQuery] ConvertCurrencyRequestDto dto)
    {
        var result = await _exchangeRateService.ConvertCurrencyAsync(dto);
        return OkResponse<ConvertCurrencyResponseDto>(result);
    }
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("History")]
    public async Task<ActionResult<PaginatedResponseViewModel<RateViewModel>>> GetHistoricalRates([FromQuery] HistoricalRatesRequestDto requestDto)
    {
        var result = await _exchangeRateService.GetHistoricalRatesAsync(requestDto);
        return PaginatedResponse<RateViewModel>(result.Data, result.TotalCount, result.PageSize, result.Page);
    }
}