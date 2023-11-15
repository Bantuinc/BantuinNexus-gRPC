using AutoMapper;
using BantuinNexus_gRPC.Models;

namespace BantuinNexus_gRPC
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AccountDetailModel, AccountDetail>();
        }
    }
}
