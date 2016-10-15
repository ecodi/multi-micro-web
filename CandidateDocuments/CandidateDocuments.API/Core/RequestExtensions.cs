using CandidateDocuments.API.Core.Pagination;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CandidateDocuments.API.Core
{
    public static class RequestExtensions
    {
        public static PaginationRequestHeader GetPagination(this HttpRequest request)
        {
            var pagination = request.Headers["Pagination"].ToString();
            return pagination == null ? null : JsonConvert.DeserializeObject<PaginationRequestHeader>(pagination);
        }
        public static void AddPagination(this HttpResponse response, PaginationResponseHeader pagination)
        {
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(pagination));
            response.Headers.Add("access-control-expose-headers", "Pagination");
        }

        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("access-control-expose-headers", "Application-Error");
        }
    }
}
