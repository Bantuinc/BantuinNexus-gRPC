using Grpc.Core;

namespace BantuinNexus_gRPC.Services
{
    public class FileTransferService : FileService.FileServiceBase
    {
        public override async Task<FileResponse> UploadFile(IAsyncStreamReader<FileRequest> requestStream, ServerCallContext context)
        {
            var filePath = Path.Combine("wwwroot", "uploads");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            while (await requestStream.MoveNext())
            {
                var request = requestStream.Current;
                var fileFullPath = Path.Combine(filePath, request.FileName);

                using (var fileStream = File.Create(fileFullPath))
                {
                    request.FileContent.WriteTo(fileStream);
                }
            }

            return new FileResponse { Message = "File successfully uploaded." };
        }
    }
}
