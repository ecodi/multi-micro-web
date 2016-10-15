using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace CandidateDocuments.API.Core.Pagination
{
    /// <summary>
    /// Extends Swagger documentation with "Pagination" in request and response headers.
    /// Applies to actions marked with "PaginationActionFilter" attribute.
    /// </summary>
    public class PaginationHeaderOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return;
            var attrs = controllerActionDescriptor.MethodInfo.CustomAttributes;
            if (attrs.All(a => a.AttributeType != typeof(PaginationActionFilter))) return;

            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "Pagination",
                In = "header",
                Required = false,
                Type = nameof(PaginationRequestHeader)

            });

            foreach (var response in operation.Responses.Select(x => x.Value))
            {
                if (response.Headers == null) response.Headers = new Dictionary<string, Header>();
                response.Headers.Add("Pagination", new Header { Type = nameof(PaginationResponseHeader) });
            }
        }
    }
}
