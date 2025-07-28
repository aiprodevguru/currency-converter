using CurrencyConverter.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Helpers
{
    public static class ResponseHelper
    {
       
        public static DataResponseViewModel<T> Data<T>(T data, bool success,  string? message = null)
        {
            return new DataResponseViewModel<T>
            {
                Success = success,
                Message = message ?? "Request successful.",
                Data = data
            };
        }

        public static NoContentResponseViewModel NoData(bool success, string errorMessage)
        {
            return new NoContentResponseViewModel
            {
                Success = success,
                Message = errorMessage
            };
        }
      
      
        public static PaginatedResponseViewModel<T> Paginate<T>(int totalCount, int pageSize, int currentPage, IEnumerable<T> data, string? message = null)
        {
            return new PaginatedResponseViewModel<T>
            {
                Success = true,
                Message = message ?? "Request successful.",
                Data = data,
                Pagination = new PaginationViewModel
                {
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = currentPage,
                }
            };
        }

    }
    
}
