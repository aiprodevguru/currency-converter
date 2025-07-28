using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.ViewModels;
using CurrencyConverter.Helpers;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<DataResponseViewModel<T>> OkResponse<T>(T data, string? message = null)
        {
            return Ok(ResponseHelper.Data(data, true, message));
        }

        protected ActionResult<DataResponseViewModel<T>> BadRequestResponse<T>(string message)
        {
            return BadRequest(ResponseHelper.NoData(false,message));
        }

        protected IActionResult ProblemResponse(string message, int statusCode = 500)
        {
            return StatusCode(statusCode, ResponseHelper.NoData(false, message));
        }

        protected IActionResult CreatedResponse(string uri, object data, string? message = null)
        {
            return Created(uri, ResponseHelper.Data(data, true, message));
        }

        protected IActionResult NotFoundResponse(string message = "Resource not found.")
        {
            return NotFound(ResponseHelper.NoData(false, message));
        }

        protected ActionResult<PaginatedResponseViewModel<T>> PaginatedResponse<T>(IEnumerable<T> items, int totalCount, int pageSize, int currentPage, string? message = null)
        {
            return Ok(ResponseHelper.Paginate(totalCount, pageSize, currentPage, items, message));
        }
    }
}
