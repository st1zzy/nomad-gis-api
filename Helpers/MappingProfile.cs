using AutoMapper;
using NetTopologySuite;
using NetTopologySuite.Geometries; 
using nomad_gis_V2.DTOs.Achievements;
using nomad_gis_V2.DTOs.Auth;
using nomad_gis_V2.DTOs.Messages;
using nomad_gis_V2.DTOs.Points;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Helpers
{
    public class MappingProfile : AutoMapper.Profile
    {
        private readonly GeometryFactory _geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        
        public MappingProfile()
        {
            CreateMap<MapPoint, MapPointRequest>()
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Y))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.X));

            CreateMap<MapPointCreateRequest, MapPoint>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                    _geometryFactory.CreatePoint(new Coordinate(src.Longitude, src.Latitude))
                ));
            
            CreateMap<MapPointUpdateRequest, MapPoint>()
                 .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                    _geometryFactory.CreatePoint(new Coordinate(src.Longitude, src.Latitude))
                ));

            CreateMap<Achievement, AchievementResponse>();

            CreateMap<User, UserDto>();
            
            CreateMap<Message, MessageResponse>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.User.AvatarUrl));
        }
    }
}