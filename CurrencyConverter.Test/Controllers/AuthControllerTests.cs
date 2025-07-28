using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Services;
using CurrencyConverter;
using CurrencyConverter.DTOs;
using CurrencyConverter.ViewModels;
namespace CurrencyConverter.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly AuthController _controller;
        private readonly Mock<IAuthService> _mockAuthService;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public void Login_ReturnsOk_WithToken()
        {
            // Arrange
            var loginRequest = new LoginRequestDto { Username = "admin", Password = "1234" };
            var expectedToken = "mock-jwt-token";

            _mockAuthService
                .Setup(s => s.Authenticate(loginRequest))
                .Returns(expectedToken);

            // Act
            var result = _controller.Login(loginRequest);

            // Assert
            var actionResult = Assert.IsType<ActionResult<DataResponseViewModel<LoginResponseDto>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<DataResponseViewModel<LoginResponseDto>>(okResult.Value);

            Assert.Equal(expectedToken, response.Data.Token);
        }
    }

}
