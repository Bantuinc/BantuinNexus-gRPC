using BantuinNexus_gRPC;
using Google.Protobuf.WellKnownTypes;
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
            var response = new LoginRes();
            if (queryUser != null)
            {
                if(BC.Verify(request.Password, queryUser.password))
                {
                    IDictionary<string, object> authResponse = JwtAuthenticationManager.Login(queryUser.name);
                    response = new LoginRes
                    {
                        Account = new AccountDetail
                        {
                            Id = queryUser.id,
                            Name = queryUser.name,
                            Email = queryUser.email,
                            Status = queryUser.status,
                            Role = 1
                        },
                        Token = new LoginRes.Types.Token
                        {
                            AccessToken = (string)authResponse["token"],
                            Expires = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + Convert.ToInt32((double)authResponse["expired"])
                        },
                        Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    };
                    
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
            return response;
            
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
                        activation_token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                        role = 1,
                        status = 1,
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
