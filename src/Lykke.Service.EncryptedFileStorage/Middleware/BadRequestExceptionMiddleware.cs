using System.Net;
using System.Threading.Tasks;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.EncryptedFileStorage.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.EncryptedFileStorage.Middleware
{
    public class BadRequestExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public BadRequestExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        [UsedImplicitly]
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (BadRequestException e)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                var json = ErrorResponse.Create(e.Message).ToJson();
                await context.Response.WriteAsync(json);
            }
        }
    }
}
