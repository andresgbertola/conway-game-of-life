using AutoMapper;
using GOL.Application.DTOs;
using GOL.Domain.Entities;

namespace GOL.Application.Mapper
{
    public class BoardStateMappingProfile : Profile
    {
        public BoardStateMappingProfile()
        {
            CreateMap<BoardState, BoardStateDto>()
                .ForMember(dest => dest.LiveCells, opt => opt.MapFrom(src => src.LiveCells))
                .ForMember(dest => dest.Iteration, opt => opt.MapFrom(src => src.Iteration))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
