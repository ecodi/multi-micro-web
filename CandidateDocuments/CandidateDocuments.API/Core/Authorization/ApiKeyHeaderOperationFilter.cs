using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace CandidateDocuments.API.Core.Authorization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiKeyHeader : Attribute
    { }

    /// <summary>
    /// Extends Swagger documentation with "Api-Key" in request headers.
    /// Applies to actions of controllers marked with "ApiKeyHeader" attribute.
    /// </summary>
    public class ApiKeyHeaderOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var controllerActionDescriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor == null) return;
            var attrs = controllerActionDescriptor.ControllerTypeInfo.CustomAttributes;
            if (attrs.All(a => a.AttributeType != typeof(ApiKeyHeader))) return;

            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();

            operation.Parameters.Add(new NonBodyParameter
            {
                Name = "Api-Key",
                In = "header",
                Required = true,
                Type = "string"
            });
        }
    }
}
