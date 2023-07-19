using BantuinNexus_gRPC;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace BantuinNexus_gRPC.Services
{
    /*[Authorize]*/
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override async Task SayHello(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            for (int i = 0; i < 30; i++)
            {
                await responseStream.WriteAsync(new HelloReply
                {
                    Message = "Hello " + request.Name,
                    Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
                });
                await Task.Delay(1000);

            }
        }
        public override async Task<Tes> SayHelloAgain(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            var response = new Tes();
            await foreach (var message in requestStream.ReadAllAsync())
            {
                response.Reply.Add(new HelloReply
                {
                    Message = "Hello " + message.Name,
                    Timestamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }
            return response;
        }
    }
}