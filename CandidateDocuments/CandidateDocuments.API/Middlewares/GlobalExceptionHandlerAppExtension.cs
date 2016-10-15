using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CandidateDocuments.API.Core;
using CandidateDocuments.Application.Core;

namespace CandidateDocuments.API.Middlewares
{
    public static class GlobalExceptionHandlerAppExtension
    {
        public static void UseGlobalExceptionHanlder(this IApplicationBuilder app, bool isDevelopment)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var logger = serviceScope.ServiceProvider.GetService<ILogger<Startup>>();
                app.UseExceptionHandler(
                    builder =>
                    {
                        builder.Run(
                            async context =>
                            {
                                var excHandler = context.Features.Get<IExceptionHandlerFeature>();
                                var statusCode = (int)HttpStatusCode.InternalServerError;
                                if (excHandler != null)
                                {
                                    var error = excHandler.Error;
                                    logger.LogError(error.Message);
                                    if (error is AuthenticationException)
                                        statusCode = (int)HttpStatusCode.Unauthorized;
                                    else if (error is UniqnessViolation)
                                        statusCode = (int)HttpStatusCode.Conflict;
                                    else
                                    {
                                        logger.LogError(error.Message);
                                        var msg = isDevelopment ? error.Message : ResponseMessageCodes.InternalError;
                                        context.Response.AddApplicationError(msg);
                                        await context.Response.WriteAsync(msg).ConfigureAwait(false);
                                    }
                                }
                                context.Response.StatusCode = statusCode;
                                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                            });
                    });
            }
        }
    }
}
