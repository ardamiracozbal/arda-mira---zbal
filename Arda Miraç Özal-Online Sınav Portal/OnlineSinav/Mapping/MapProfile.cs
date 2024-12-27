using OnlineSinav.Models;
using AutoMapper;
using OnlineSinav.ViewModel;

namespace OnlineSinav.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
           
            CreateMap<AppUser,LoginViewModel>().ReverseMap();
            CreateMap<AppUser,RegisterViewModel>().ReverseMap();

       
          
        }
    }
}
