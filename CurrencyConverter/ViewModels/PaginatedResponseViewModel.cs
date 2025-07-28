namespace CurrencyConverter.ViewModels
{
    public class PaginatedResponseViewModel<T>
    {
        public bool Success { get; set; } = true;
        public required string Message { get; set; }
        public IEnumerable<T> Data { get; set; } = new List<T>();

        public required PaginationViewModel Pagination { get; set; } 
       
    }

    public class PaginationViewModel
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double) PageSize);
        public bool HasNext => CurrentPage < TotalPages;
        public bool HasPrevious => CurrentPage > 1;
    }
}
