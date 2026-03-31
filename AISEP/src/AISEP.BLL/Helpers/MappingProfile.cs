using AISEP.BLL.Helpers;
using AutoMapper;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;

namespace AISEP.BLL.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Advisor Entity → AdvisorResponse
            CreateMap<Advisor, AdvisorResponse>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.Industry,
                    opt => opt.MapFrom(src => src.Industry != null ? src.Industry.ToString() : null))
                .ForMember(dest => dest.ApprovalStatus,
                    opt => opt.MapFrom(src => src.ApprovalStatus.ToString()));

            // CreateAdvisorRequest → Advisor Entity
            CreateMap<CreateAdvisorRequest, Advisor>()
                .ForMember(dest => dest.AdvisorId,       opt => opt.Ignore())
                .ForMember(dest => dest.UserId,          opt => opt.Ignore())
                .ForMember(dest => dest.Rating,          opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalStatus,  opt => opt.Ignore())
                .ForMember(dest => dest.User,            opt => opt.Ignore())
                .ForMember(dest => dest.Bookings,        opt => opt.Ignore())
                .ForMember(dest => dest.Reviews,         opt => opt.Ignore())
                .ForMember(dest => dest.Wallet,          opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImage,    opt => opt.Ignore())
                .ForMember(dest => dest.Certifications,  opt => opt.Ignore())
                .ForMember(dest => dest.HourlyRate,
                    opt => opt.MapFrom(src => src.HourlyRate > 0 ? src.HourlyRate : null));

            // UpdateAdvisorRequest → Advisor Entity
            CreateMap<UpdateAdvisorRequest, Advisor>()
                .ForMember(dest => dest.AdvisorId,       opt => opt.Ignore())
                .ForMember(dest => dest.UserId,          opt => opt.Ignore())
                .ForMember(dest => dest.Rating,          opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalStatus,  opt => opt.Ignore())
                .ForMember(dest => dest.User,            opt => opt.Ignore())
                .ForMember(dest => dest.Bookings,        opt => opt.Ignore())
                .ForMember(dest => dest.Reviews,         opt => opt.Ignore())
                .ForMember(dest => dest.Wallet,          opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImage,    opt => opt.Ignore())
                .ForMember(dest => dest.Certifications,  opt => opt.Ignore());

            // Document Entity → DocumentResponse
            CreateMap<Document, DocumentResponse>()
                .ForMember(dest => dest.DocumentType,
                    opt => opt.MapFrom(src => src.DocumentType.ToString()));

            // Booking Entity → BookingResponse
            CreateMap<Booking, BookingResponse>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Advisor != null && src.Advisor.User != null
                        ? src.Advisor.User.UserName
                        : "Unknown"))
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Project != null
                        ? src.Project.ProjectName
                        : "Unknown"))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null
                        ? src.Customer.UserName
                        : "Unknown"))
                .ForMember(dest => dest.AdvisorAvailabilitySlotIds,
                    opt => opt.MapFrom(src => src.BookingSlots.Select(bs => bs.AdvisorAvailabilityId)))
                .ForMember(dest => dest.SlotCount,
                    opt => opt.MapFrom(src => src.BookingSlots.Count));

            // AdvisorAvailability mappings
            CreateMap<CreateAdvisorAvailabilityRequest, AdvisorAvailability>()
                .ForMember(dest => dest.AdvisorAvailabilityId, opt => opt.Ignore())
                .ForMember(dest => dest.AdvisorId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Advisor, opt => opt.Ignore())
                .ForMember(dest => dest.BookingSlots, opt => opt.Ignore());

            CreateMap<AdvisorAvailability, AdvisorAvailabilityResponse>();

            // CreateConsultingReportRequest -> ConsultingReport Entity
            CreateMap<CreateConsultingReportRequest, ConsultingReport>()
                .ForMember(dest => dest.ConsultingReportId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore());

            // ConsultingReport Entity -> ConsultingReportResponse
            CreateMap<ConsultingReport, ConsultingReportResponse>()
                .ForMember(dest => dest.AdvisorId, opt => opt.MapFrom(src => src.Booking.AdvisorId))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Booking.CustomerId))
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Booking.Advisor != null && src.Booking.Advisor.User != null
                        ? src.Booking.Advisor.User.UserName
                        : "Unknown"))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Booking.Customer != null
                        ? src.Booking.Customer.UserName
                        : "Unknown"));

            // User Entity → UserResponse
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id));

            // Startup Entity → StartupResponse
            CreateMap<Startup, StartupResponse>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.StartupId))
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Industry,
                    opt => opt.MapFrom(src => src.Industry != null ? src.Industry.ToString() : null))
                .ForMember(dest => dest.ApprovalStatus,
                    opt => opt.MapFrom(src => src.ApprovalStatus.ToString()))
                .ForMember(dest => dest.FollowerCount,
                    opt => opt.MapFrom(src => src.Projects != null
                        ? src.Projects
                            .SelectMany(p => p.Followers)
                            .Select(f => f.FollowerId)
                            .Distinct()
                            .Count()
                        : 0))
                    //opt => opt.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.ApprovedById,    opt => opt.MapFrom(src => src.ApprovedById))
                .ForMember(dest => dest.ApprovedAt,      opt => opt.MapFrom(src => src.ApprovedAt))
                .ForMember(dest => dest.RejectedById,    opt => opt.MapFrom(src => src.RejectedById))
                .ForMember(dest => dest.RejectedAt,      opt => opt.MapFrom(src => src.RejectedAt))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason));

            // Investor Entity → InvestorResponse
            CreateMap<Investor, InvestorResponse>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));

            // CreateInvestorRequest → Investor Entity
            CreateMap<CreateInvestorRequest, Investor>()
                .ForMember(dest => dest.InvestorId,         opt => opt.Ignore())
                .ForMember(dest => dest.UserId,             opt => opt.Ignore())
                .ForMember(dest => dest.User,               opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals,              opt => opt.Ignore())
                .ForMember(dest => dest.InvestorAIAnalyses, opt => opt.Ignore())
                .ForMember(dest => dest.InvestmentAmount,
                    opt => opt.MapFrom(src => src.InvestmentAmount > 0 ? src.InvestmentAmount : null));

            // UpdateInvestorRequest → Investor Entity
            CreateMap<UpdateInvestorRequest, Investor>()
                .ForMember(dest => dest.InvestorId,         opt => opt.Ignore())
                .ForMember(dest => dest.UserId,             opt => opt.Ignore())
                .ForMember(dest => dest.User,               opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals,              opt => opt.Ignore())
                .ForMember(dest => dest.InvestorAIAnalyses, opt => opt.Ignore());

            // ProjectFollower Entity → FollowedProjectResponse
            CreateMap<ProjectFollower, FollowedProjectResponse>()
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : "Unknown"))
                .ForMember(dest => dest.ProjectImageUrl,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectImageUrl : null))
                .ForMember(dest => dest.Industry,
                    opt => opt.MapFrom(src => src.Project.Industry.ToString()))
                .ForMember(dest => dest.FollowedAt,
                    opt => opt.MapFrom(src => src.CreatedAt));

            // Project Entity → ProjectResponse
            CreateMap<Project, ProjectResponse>()
                .ForMember(dest => dest.DevelopmentStage,
                    opt => opt.MapFrom(src => src.DevelopmentStage != null ? src.DevelopmentStage.ToString() : null))
                .ForMember(dest => dest.Industry,
                    opt => opt.MapFrom(src => src.Industry.ToString()))
                .ForMember(dest => dest.StartupPotentialScore,
                    opt => opt.MapFrom(src => src.StartupAIAnalysis != null ? src.StartupAIAnalysis.PotentialScore : null))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ApprovedById, opt => opt.MapFrom(src => src.ApprovedById))
                .ForMember(dest => dest.ApprovedAt, opt => opt.MapFrom(src => src.ApprovedAt))
                .ForMember(dest => dest.RejectedById, opt => opt.MapFrom(src => src.RejectedById))
                .ForMember(dest => dest.RejectedAt, opt => opt.MapFrom(src => src.RejectedAt))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason));

            // Project Entity → NonPremiumProjectResponse
            CreateMap<Project, NonPremiumProjectResponse>()
                .ForMember(dest => dest.StartupId,
                    opt => opt.MapFrom(src => src.StartupId))
                .ForMember(dest => dest.DevelopmentStage,
                    opt => opt.MapFrom(src => src.DevelopmentStage != null ? src.DevelopmentStage.ToString() : null))
                .ForMember(dest => dest.Industry,
                    opt => opt.MapFrom(src => src.Industry.ToString()))
                .ForMember(dest => dest.StartupPotentialScore,
                    opt => opt.MapFrom(src => src.StartupAIAnalysis != null ? src.StartupAIAnalysis.PotentialScore : null));

            // CreateProjectRequest -> Project Entity
            CreateMap<CreateProjectRequest, Project>()
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.StartupId, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedById, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
                .ForMember(dest => dest.Startup, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.StartupAIAnalysis, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorAIAnalyses, opt => opt.Ignore())
                .ForMember(dest => dest.UnlockedProjects, opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals, opt => opt.Ignore());

            // UpdateProjectRequest -> Project Entity
            var updateProjectMap = CreateMap<UpdateProjectRequest, Project>()
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.StartupId, opt => opt.Ignore())
                .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedById, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedById, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectionReason, opt => opt.Ignore())
                .ForMember(dest => dest.Startup, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.StartupAIAnalysis, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorAIAnalyses, opt => opt.Ignore())
                .ForMember(dest => dest.UnlockedProjects, opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals, opt => opt.Ignore());

            updateProjectMap.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // StartupAIAnalysis Entity → StartupAIAnalysisResponse
            CreateMap<StartupAIAnalysis, StartupAIAnalysisResponse>()
                .ForMember(dest => dest.Analysis, opt => opt.Ignore())
                .ForMember(dest => dest.ScoreBreakdown, opt => opt.Ignore());

            // InvestorAIAnalysis Entity → InvestorAIAnalysisResponse
            CreateMap<InvestorAIAnalysis, InvestorAIAnalysisResponse>()
                .ForMember(dest => dest.Analysis, opt => opt.Ignore())
                .ForMember(dest => dest.PotentialScore, opt => opt.Ignore())
                .ForMember(dest => dest.ChaosScore, opt => opt.Ignore())
                .ForMember(dest => dest.ScoreBreakdown, opt => opt.Ignore());

            // StartupAIAnalysis Entity -> StartupEligibilityResponse
            CreateMap<StartupAIAnalysis, StartupEligibilityResponse>()
                .ForMember(dest => dest.IsEligibleStartup,
                    opt => opt.MapFrom(src => src.IsEligibleStartup ?? false))
                .ForMember(dest => dest.EligibilityReason,
                    opt => opt.MapFrom(src => src.EligibilityReason ?? string.Empty));

            // Notification Entity -> NotificationDto
            CreateMap<Notification, NotificationDto>();

            // NFTRecord Entity -> NFTRecordDto
            CreateMap<NFTRecord, NFTRecordDto>()
                .ForMember(dest => dest.ValidityStatus,
                    opt => opt.MapFrom(src => src.ValidityStatus.ToString()));

            // Deal Entity -> DealDto
            CreateMap<Deal, DealDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            // Deal Entity -> DealContractStatusResponse
            CreateMap<Deal, DealContractStatusResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.IsContractSigned,
                    opt => opt.MapFrom(src => src.Status == AISEP.DAL.Enums.DealStatus.Contract_Signed || src.Status == AISEP.DAL.Enums.DealStatus.Minted_NFT));

            // CreateDealDto -> Deal Entity
            CreateMap<CreateDealDto, Deal>()
                .ForMember(dest => dest.DealId, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorId, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.StartupConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DealDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.CompletionDate, opt => opt.Ignore())
                .ForMember(dest => dest.Investor, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.NFTRecord, opt => opt.Ignore());

            // ConnectionRequest Entity -> ConnectionRequestDto
            CreateMap<ConnectionRequest, ConnectionRequestDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ChatSessionId,
                    opt => opt.MapFrom(src => src.ChatSession != null ? (int?)src.ChatSession.ChatSessionId : null));

            // Startup Entity -> ContactInfoDto
            CreateMap<Startup, ContactInfoDto>();
        }
    }
}

