using BantuinNexus_gRPC;
using BantuinNexus_gRPC.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;
using SqlKata.Execution;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

/*
 * * Protocol negotiation
 * * Konfigurasi Kestrel bisa diakses 2 protocol, http1 dan http2
 * */
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1);
    options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http2);

});

/* 
 * * Untuk authentikasi dengan JWT
 * */
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(SettingsConfigHelper.AppSetting("Jwt:SecretKey")))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddGrpc();

/*
 * * Untuk mengaktifkan GrpcWeb
 * */
builder.Services.AddTransient<QueryFactory>((e) =>
{
    var connection = new MySqlConnection(SettingsConfigHelper.AppSetting("ConnectionStrings:MySql"));
    var compiler = new MySqlCompiler();
    return new QueryFactory(connection, compiler);
});

/*
 * * Untuk setting CORS
 * */
builder.Services.AddCors(builder =>
{
    builder.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
    /*builder.AddPolicy("AllowGrpcWebClient", policy =>
    {
        policy.WithOrigins("https://localhost:7295")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });*/
});

// build
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors("AllowAll");
/*app.UseCors("AllowGrpcWebClient");*/
app.UseAuthentication();
app.UseAuthorization();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<GreeterService>().EnableGrpcWeb();
app.MapGrpcService<AccountService>().EnableGrpcWeb();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
