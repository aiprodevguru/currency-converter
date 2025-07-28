using CurrencyConverter.Services;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.DTOs;
using CurrencyConverter.ViewModels;
using CurrencyConverter.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<DataResponseViewModel<LoginResponseDto>> Login([FromBody] LoginRequestDto requestDto)
    {
        var Token = _authService.Authenticate(requestDto);
        return OkResponse<LoginResponseDto>(new LoginResponseDto { Token=Token });
    }
}