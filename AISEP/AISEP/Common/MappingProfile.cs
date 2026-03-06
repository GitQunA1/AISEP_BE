using AutoMapper;
using AISEP.Models.DTOs;
using AISEP.Models.Entities;
using AISEP.DTOs;
using AISEP.Models.Enums;

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
                .ForMember(dest => dest.ChatSession, opt => opt.Ignore())
                .ForMember(dest => dest.ConsultingReport, opt => opt.Ignore());

            // User Entity → UserResponseDto
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id));

            // Startup Entity → StartupResponseDto
            CreateMap<Startup, StartupResponseDto>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.StartupId))
                .ForMember(dest => dest.Industry,
                    opt => opt.MapFrom(src => src.Industry != null ? src.Industry.ToString() : null))
                .ForMember(dest => dest.ApprovalStatus,
                    opt => opt.MapFrom(src => src.ApprovalStatus.ToString()))
                .ForMember(dest => dest.FollowerCount,
                    opt => opt.MapFrom(src => src.Followers != null ? src.Followers.Count : 0));

            // Investor Entity → InvestorResponseDto
            CreateMap<Investor, InvestorResponseDto>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));

            // InvestorDto → Investor Entity
            CreateMap<InvestorDto, Investor>()
                .ForMember(dest => dest.InvestorId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorAIAnalyses, opt => opt.Ignore());

            // StartupFollower Entity → FollowedStartupDto
            CreateMap<StartupFollower, FollowedStartupDto>()
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src => src.Startup != null ? src.Startup.CompanyName : "Unknown"))
                .ForMember(dest => dest.LogoUrl,
                    opt => opt.MapFrom(src => src.Startup != null ? src.Startup.LogoUrl : null))
                .ForMember(dest => dest.Industry,
                    opt => opt.MapFrom(src => src.Startup != null ? src.Startup.Industry : null));

            // Project Entity → ProjectResponseDto
            CreateMap<Project, ProjectResponseDto>()
                .ForMember(dest => dest.DevelopmentStage,
                    opt => opt.MapFrom(src => src.DevelopmentStage != null ? src.DevelopmentStage.ToString() : null))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
