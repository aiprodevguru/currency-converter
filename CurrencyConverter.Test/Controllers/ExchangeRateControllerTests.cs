using Xunit;
using Moq;
using CurrencyConverter;
using CurrencyConverter.Services;
using CurrencyConverter.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CurrencyConverter.DTOs;
using CurrencyConverter.ViewModels;

namespace CurrencyConverter.Tests.Controllers
{
    public class ExchangeRateControllerTests
    {
        private readonly Mock<IExchangeRateService> _mockService;
        private readonly ExchangeRateController _controller;

        public ExchangeRateControllerTests()
        {
            _mockService = new Mock<IExchangeRateService>();
            _controller = new ExchangeRateController(_mockService.Object);
        }

        [Fact]
        public async Task GetLatestRates_ReturnsOkResult_WithRates()
        {
            var mockRates = new LatestRateResponseDto()
            {
                Base="EUR",
                Date= DateOnly.FromDateTime(DateTime.Today),
                Rates= new Dictionary<string, decimal>() { { "USD", 1.0m }, { "EUR", 0.85m } }
            };
            var dto = new LatestRateRequestDto() { Base = "EUR" };
            _mockService.Setup(s => s.GetLatestRatesAsync(dto)).ReturnsAsync(mockRates);

            var result = await _controller.GetLatestRates(dto);

            var actionResult = Assert.IsType<ActionResult<DataResponseViewModel<LatestRateResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<DataResponseViewModel<LatestRateResponseDto>>(okResult.Value);
            Assert.Equal(mockRates, response.Data);
        }

        [Fact]
        public async Task ConvertCurrency_ReturnsOkResult_WithAmount()
        {
            var convertedAmount = new ConvertCurrencyResponseDto() {
                Amount = 12.1F,
                Base = "USD",
                Date = DateOnly.FromDateTime(DateTime.Today),
                Rates = new Dictionary<string, decimal>()
            };
            var requestDto = new ConvertCurrencyRequestDto() { From="USD", To="EUR", Amount=100 };

            _mockService.Setup(s => s.ConvertCurrencyAsync(requestDto))
                        .ReturnsAsync(convertedAmount);

            var result = await _controller.ConvertCurrency(requestDto);

            var actionResult = Assert.IsType<ActionResult<DataResponseViewModel<ConvertCurrencyResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<DataResponseViewModel<ConvertCurrencyResponseDto>>(okResult.Value);
            Assert.Equal(convertedAmount, response.Data);
        }


        [Fact]
        public async Task GetHistoricalRates_ReturnsOkResult_WithData()
        {
            var history = new HistoricalRatesViewModel
            {
                Base="USD",
                StartDate=DateOnly.Parse("2025-01-01"),
                EndDate= DateOnly.Parse("2025-05-01"),
                Amount=100,
                Page=1,
                PageSize=10,
                TotalCount=100,
                TotalPages=10,
                Data = new List<RateViewModel>
                {
                    new RateViewModel {Date=DateOnly.Parse("2025-01-02"), Rate=new Dictionary<string, decimal> { {"EUR", 100.0m } } },
                    new RateViewModel {Date=DateOnly.Parse("2025-01-05"), Rate=new Dictionary<string, decimal> { {"EUR", 10.0m } } },
                }
            };

            var requestDto = new HistoricalRatesRequestDto
            {
                BaseCurrency = "EUR",
                Start = DateOnly.Parse("2025-01-01"),
                End = DateOnly.Parse("2025-01-05"),
                Page = 1,
                PageSize=10,
                Provider = "frankfurter"
            };
            _mockService.Setup(s => s.GetHistoricalRatesAsync(requestDto))
                        .ReturnsAsync(history);

            var result = await _controller.GetHistoricalRates(requestDto);

            var actionResult = Assert.IsType<ActionResult<PaginatedResponseViewModel<RateViewModel>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<PaginatedResponseViewModel<RateViewModel>>(okResult.Value);
            
            Assert.Equivalent(new PaginatedResponseViewModel<RateViewModel> { 
                Data = new List<RateViewModel>
                {
                    new RateViewModel {Date=DateOnly.Parse("2025-01-02"), Rate=new Dictionary<string, decimal> { {"EUR", 100.0m } } },
                    new RateViewModel {Date=DateOnly.Parse("2025-01-05"), Rate=new Dictionary<string, decimal> { {"EUR", 10.0m } } },
                },
                Message= "Request successful.",
                Success = true,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalCount=100
                },
            }, response);
        }
    }
}
