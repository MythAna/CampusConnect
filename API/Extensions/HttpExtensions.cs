using System.Text.Json;
using API.Helpers;

namespace API.Extensions
{
    public  static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int CurrentPage, int PageSize, int totalCount, int TotalPages, PaginationHeader header)
        {
            var jsonOptions = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions));
            response.Headers.Add("Access-Control-Expose-Header", "Pagination");
        }
    }
}