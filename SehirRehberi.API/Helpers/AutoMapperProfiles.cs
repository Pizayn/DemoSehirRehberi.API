using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using SehirRehberi.API.Dtos;
using SehirRehberi.API.Models;

namespace SehirRehberi.API.Helpers
{
    public class AutoMapperProfiles:Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<City, CityForListDto>()
                .ForMember(dest=>dest.PhotoUrl, opt =>
                {
                    opt.MapFrom(src=>src.Photos.FirstOrDefault(p=>p.IsMain).Url);
                });
            //photourl için kaynaktaki photoların içindeki url leri al.
            CreateMap<City, CityForDetailDto>();//böle yaparsak sadece isimleri dtoda eşleşenlere map eder
            CreateMap<PhotoForCreationDto,Photo>();
            CreateMap<PhotoForReturnDto, Photo > ();
        }
    }
}
