using AutoMapper;
using AISEP.Models.DTOs;
using AISEP.Models.Entities;
using AISEP.DTOs;

namespace AISEP.Common
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Document Entity → DocumentResponseDto
            CreateMap<Document, DocumentResponseDto>()
                .ForMember(dest => dest.DocumentType,
                    opt => opt.MapFrom(src => src.DocumentType.ToString()));

            // Booking Entity → BookingResponseDto
            CreateMap<Booking, BookingResponseDto>()
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Advisor != null && src.Advisor.User != null
                        ? src.Advisor.User.UserName
                        : "Unknown"))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null
                        ? src.Customer.UserName
                        : "Unknown"));

            // BookingDto → Booking Entity
            CreateMap<BookingDto, Booking>()
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.Advisor, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ChatSessions, opt => opt.Ignore())
                .ForMember(dest => dest.ConsultingReports, opt => opt.Ignore());

            // User Entity → UserResponseDto
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id));
        }
    }
}
