using BantuinNexus_gRPC;
using Grpc.Core;
using SqlKata.Execution;
using BC = BCrypt.Net.BCrypt;

namespace BantuinNexus_gRPC.Services
{
    public class AccountService : Account.AccountBase
    {
        private readonly ILogger<AccountService> _logger;
        private readonly QueryFactory _db;

        public AccountService(ILogger<AccountService> logger, QueryFactory db)
        {
            _logger = logger;
            _db = db;
        }
        public override async Task<LoginRes> Login(LoginReq request, ServerCallContext context)
        {

            var queryUser = _db.Query("users").Where("email", request.Email.ToLower()).FirstOrDefault();
            var authResponse = new LoginRes();
            if (queryUser != null)
            {
                if(BC.Verify(request.Password, queryUser.password))
                {
                    authResponse = JwtAuthenticationManager.Login(queryUser.name);
                }
                else
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Email atau Password Salah"));
                }
            }
            else
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Email atau Password Salah"));
            }
            return authResponse;
            
        }

        public override async Task<RegisterRes> Register(RegisterReq request, ServerCallContext context)
        {
            var regResponse = new RegisterRes();
            var queryUser = _db.Query("users").Where("email", request.Email.ToLower()).FirstOrDefault();
            if(queryUser == null)
            {
                try {                     
                    var queryInsert = _db.Query("users").Insert(new
                    {
                        name = request.Name,
                        email = request.Email,
                        password = BC.HashPassword(request.Password),
                    });
                    regResponse = new RegisterRes
                    {
                        Name = request.Name,
                        Status = "Registrasi Berhasil"
                    };
                }
                catch (Exception ex)
                {
                    throw new RpcException(new Status(StatusCode.Internal, ex.Message));
                }
            }
            else
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email Sudah Terdaftar"));
            }
            return regResponse;
        }
    }
}
