namespace API.Helpers
{
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pagesize = 10;
        public int PageSize
        {
            get => _pagesize;
            set => _pagesize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}