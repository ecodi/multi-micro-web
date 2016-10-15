namespace CandidateDocuments.API.Core.Pagination
{
    public class PaginationRequestHeader
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
    }

    public class PaginationResponseHeader : PaginationRequestHeader
    {
        public int? TotalItems { get; set; }
        public int? TotalPages { get; set; }
        public int Offset => (CurrentPage - 1)*ItemsPerPage;
    }
}
