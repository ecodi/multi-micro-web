using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CandidateDocuments.API.Core.Pagination
{
    /// <summary>
    /// Retrieves pagination parameters from request headers.
    /// Extends response headers with supplemented pagination info.
    /// </summary>
    public class PaginationActionFilter : ActionFilterAttribute
    {
        public int DefaultPage { get; set; } = 1;
        public int DefaultItemsPerPage { get; set; } = 10;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var controller = filterContext.Controller as Controller;
            if (controller == null) return;
            var pagination = filterContext.HttpContext.Request.GetPagination();
            controller.ViewBag.PaginationHeader = new PaginationResponseHeader
            {
                CurrentPage = pagination?.CurrentPage ?? DefaultPage,
                ItemsPerPage = pagination?.ItemsPerPage ?? DefaultItemsPerPage,
                TotalItems = null
            };
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var controller = filterContext.Controller as Controller;
            if (controller == null) return;
            var pagination = controller.ViewBag.PaginationHeader as PaginationResponseHeader;
            if (pagination == null) return;
            pagination.TotalPages = pagination.TotalItems == null ? null : (int?)Math.Ceiling((double)pagination.TotalItems / pagination.ItemsPerPage);
            filterContext.HttpContext.Response.AddPagination(pagination);
        }
    }
}
