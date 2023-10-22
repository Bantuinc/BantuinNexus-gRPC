using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BantuinNexus_gRPC
{
    public class GrpcWebMiddleware
    {
        private readonly RequestDelegate _next;

        public GrpcWebMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("Content-Type", "application/grpc-web+proto");
            await _next(context);
        }
    }

    public static class GrpcWebMiddlewareExtensions
    {
        public static IApplicationBuilder UseGrpcWebMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GrpcWebMiddleware>();
        }
    }
}
