using Microsoft.AspNetCore.Mvc;
using CandidateDocuments.API.Core.Pagination;

namespace CandidateDocuments.API.Controllers
{
    public abstract class BaseController : Controller
    {
        protected PaginationResponseHeader PaginationHeader => ViewBag.PaginationHeader as PaginationResponseHeader;

        protected BaseController()
        {
            ViewBag.PaginationHeader = new PaginationResponseHeader();
        }
    }
}
