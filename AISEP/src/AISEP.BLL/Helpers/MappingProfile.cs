using AISEP.BLL.Helpers;
using AutoMapper;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using System.Text.Json;

namespace AISEP.BLL.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Advisor Entity ? AdvisorResponse
            CreateMap<Advisor, AdvisorResponse>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                //.ForMember(dest => dest.Industry,
                //    opt => opt.MapFrom(src => src.AdvisorIndustries
                //        .Select(ai => ai.Industry.ToString())
                //        .FirstOrDefault()))
                .ForMember(dest => dest.Industries,
                    opt => opt.MapFrom(src => src.AdvisorIndustries
                        .Select(ai => ai.IndustryOption.Value)
                        .ToList()))
                .ForMember(dest => dest.ApprovalStatus,
                    opt => opt.MapFrom(src => src.ApprovalStatus.ToString()));

            // CreateAdvisorRequest ? Advisor Entity
            CreateMap<CreateAdvisorRequest, Advisor>()
                .ForMember(dest => dest.AdvisorId,       opt => opt.Ignore())
                .ForMember(dest => dest.UserId,          opt => opt.Ignore())
                .ForMember(dest => dest.Rating,          opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalStatus,  opt => opt.Ignore())
                .ForMember(dest => dest.User,            opt => opt.Ignore())
                .ForMember(dest => dest.Bookings,        opt => opt.Ignore())
                .ForMember(dest => dest.Wallet,          opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImage,    opt => opt.Ignore())
                .ForMember(dest => dest.Certifications,  opt => opt.Ignore())
                .ForMember(dest => dest.AdvisorIndustries, opt => opt.Ignore())
                .ForMember(dest => dest.HourlyRate,
                    opt => opt.MapFrom(src => src.HourlyRate > 0 ? src.HourlyRate : null));

            // UpdateAdvisorRequest ? Advisor Entity
            CreateMap<UpdateAdvisorRequest, Advisor>()
                .ForMember(dest => dest.AdvisorId,       opt => opt.Ignore())
                .ForMember(dest => dest.UserId,          opt => opt.Ignore())
                .ForMember(dest => dest.Rating,          opt => opt.Ignore())
                .ForMember(dest => dest.ApprovalStatus,  opt => opt.Ignore())
                .ForMember(dest => dest.User,            opt => opt.Ignore())
                .ForMember(dest => dest.Bookings,        opt => opt.Ignore())
                .ForMember(dest => dest.Wallet,          opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImage,    opt => opt.Ignore())
                .ForMember(dest => dest.Certifications,  opt => opt.Ignore())
                .ForMember(dest => dest.AdvisorIndustries, opt => opt.Ignore());

            // Document Entity ? DocumentResponse
            CreateMap<Document, DocumentResponse>()
                .ForMember(dest => dest.DocumentType,
                    opt => opt.MapFrom(src => src.DocumentType.ToString()));

            // Booking Entity ? BookingResponse
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
                .ForMember(dest => dest.SystemCommissionMessage,
                    opt => opt.MapFrom(src =>
                        $"S? ti?n hoa h?ng h? th?ng AISEP nh?n đư?c cho đơn hàng này là {src.SystemCommissionAmount:0}₫. Đây là m?c hoa h?ng đư?c ch?t t?i th?i đi?m t?o booking."))
                .ForMember(dest => dest.SystemCommissionPercent,
                    opt => opt.MapFrom(src => src.SystemCommissionConfig != null ? src.SystemCommissionConfig.Percent : 0m))
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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Booking.Advisor != null && src.Booking.Advisor.User != null
                        ? src.Booking.Advisor.User.UserName
                        : "Unknown"))
                .ForMember(dest => dest.AdvisorFullName,
                    opt => opt.MapFrom(src => src.Booking.Advisor != null && src.Booking.Advisor.User != null
                        ? src.Booking.Advisor.User.FullName ?? string.Empty
                        : string.Empty))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Booking.Customer != null
                        ? src.Booking.Customer.UserName
                        : "Unknown"))
                .ForMember(dest => dest.CustomerFullName,
                    opt => opt.MapFrom(src => src.Booking.Customer != null
                        ? src.Booking.Customer.FullName ?? string.Empty
                        : string.Empty));

            // User Entity ? UserResponse
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.FullName,
                    opt => opt.MapFrom(src => src.FullName));

            CreateMap<User, AdminUserResponse>()
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.Id));

            // Startup Entity ? StartupResponse
            CreateMap<Startup, StartupResponse>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => src.StartupId))
                .ForMember(dest => dest.UserId,
                    opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Industries,
                    opt => opt.MapFrom(src => src.StartupIndustries.Select(si => si.IndustryOption.Value).ToList()))
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

            // Investor Entity ? InvestorResponse
            CreateMap<Investor, InvestorResponse>()
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.Industries,
                    opt => opt.MapFrom(src => src.InvestorIndustries.Select(ii => ii.IndustryOption.Value).ToList()))
                .ForMember(dest => dest.PreferredStageOptionId,
                    opt => opt.MapFrom(src => src.PreferredStageOptionId))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.ApprovalStatus.ToString()));

            // Package Entity -> PackageResponse
            CreateMap<Package, PackageResponse>();

            // Subscription Entity -> SubscriptionResponseDto
            CreateMap<Subscription, SubscriptionResponseDto>()
                .ForMember(dest => dest.PackageName,
                    opt => opt.MapFrom(src => src.Package != null
                        ? src.Package.PackageName
                        : string.Empty))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null
                        ? (src.User.FullName ?? src.User.UserName ?? string.Empty)
                        : string.Empty))
                .ForMember(dest => dest.UserEmail,
                    opt => opt.MapFrom(src => src.User != null
                        ? (src.User.Email ?? string.Empty)
                        : string.Empty))
                .ForMember(dest => dest.BonusFreeBookings,
                    opt => opt.MapFrom(src => src.User != null
                        ? src.User.BonusFreeBookings
                        : 0));

            // Transaction Entity -> Payment responses
            CreateMap<Transaction, CheckoutResponse>()
                .ForMember(dest => dest.PaymentCode,
                    opt => opt.MapFrom(src => src.PaymentCode ?? string.Empty))
                .ForMember(dest => dest.QrCodeUrl,
                    opt => opt.Ignore());

            CreateMap<Transaction, TransactionStatusResponse>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentCode,
                    opt => opt.MapFrom(src => src.PaymentCode ?? string.Empty));

            CreateMap<Transaction, BookingPaymentTransactionResponse>()
                .ForMember(dest => dest.BookingId,
                    opt => opt.MapFrom(src => src.ReferenceId ?? 0))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PaymentCode,
                    opt => opt.MapFrom(src => src.PaymentCode ?? string.Empty));

            // SystemTerm Entity -> SystemTermResponse
            CreateMap<SystemTerm, SystemTermResponse>();

            CreateMap<Transaction, AdminTransactionResponse>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
                .ForMember(dest => dest.UserEmail,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Email : null));

            // CreateInvestorRequest ? Investor Entity
            CreateMap<CreateInvestorRequest, Investor>()
                .ForMember(dest => dest.InvestorId,         opt => opt.Ignore())
                .ForMember(dest => dest.UserId,             opt => opt.Ignore())
                .ForMember(dest => dest.User,               opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals,              opt => opt.Ignore())
                .ForMember(dest => dest.InvestorAIAnalyses, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorIndustries, opt => opt.Ignore())
                .ForMember(dest => dest.PreferredStageOption, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageUrl,    opt => opt.Ignore())
                .ForMember(dest => dest.InvestmentAmount,
                    opt => opt.MapFrom(src => src.InvestmentAmount > 0 ? src.InvestmentAmount : null));

            // UpdateInvestorRequest ? Investor Entity
            CreateMap<UpdateInvestorRequest, Investor>()
                .ForMember(dest => dest.InvestorId,         opt => opt.Ignore())
                .ForMember(dest => dest.UserId,             opt => opt.Ignore())
                .ForMember(dest => dest.User,               opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals,              opt => opt.Ignore())
                .ForMember(dest => dest.InvestorAIAnalyses, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorIndustries, opt => opt.Ignore())
                .ForMember(dest => dest.PreferredStageOption, opt => opt.Ignore())
                .ForMember(dest => dest.ProfileImageUrl,    opt => opt.Ignore());

            // ProjectFollower Entity ? FollowedProjectResponse
            CreateMap<ProjectFollower, FollowedProjectResponse>()
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : "Unknown"))
                .ForMember(dest => dest.ProjectImageUrl,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectImageUrl : null))
                .ForMember(dest => dest.Industries,
                    opt => opt.MapFrom(src => src.Project.ProjectIndustries.Select(pi => pi.IndustryOption.Value).ToList()))
                .ForMember(dest => dest.FollowedAt,
                    opt => opt.MapFrom(src => src.CreatedAt));

            // ProjectAdvisorAssignment Entity -> ProjectAssignedAdvisorResponse
            CreateMap<ProjectAdvisorAssignment, ProjectAssignedAdvisorResponse>()
                .ForMember(dest => dest.ProjectId,
                    opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : "Unknown"))
                .ForMember(dest => dest.AdvisorId,
                    opt => opt.MapFrom(src => src.AdvisorId))
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Advisor != null && src.Advisor.User != null
                        ? src.Advisor.User.UserName
                        : $"Advisor {src.AdvisorId}"))
                .ForMember(dest => dest.AssignedAt,
                    opt => opt.MapFrom(src => src.AssignedAt));

            // Project Entity ? ProjectResponse
            CreateMap<Project, ProjectResponse>()
                .ForMember(dest => dest.StageOptionId,
                    opt => opt.MapFrom(src => src.StageOptionId))
                .ForMember(dest => dest.Industries,
                    opt => opt.MapFrom(src => src.ProjectIndustries.Select(pi => pi.IndustryOption.Value).ToList()))
                .ForMember(dest => dest.StartupPotentialScore,
                    opt => opt.MapFrom(src => src.StartupAIAnalysis != null ? src.StartupAIAnalysis.PotentialScore : null))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ApprovedById, opt => opt.MapFrom(src => src.ApprovedById))
                .ForMember(dest => dest.ApprovedAt, opt => opt.MapFrom(src => src.ApprovedAt))
                .ForMember(dest => dest.RejectedById, opt => opt.MapFrom(src => src.RejectedById))
                .ForMember(dest => dest.RejectedAt, opt => opt.MapFrom(src => src.RejectedAt))
                .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason))
                .ForMember(dest => dest.FollowerCount,
                    opt => opt.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.IsFollowedByCurrentUser,
                    opt => opt.MapFrom((src, _, _, context) =>
                        context.TryGetItems(out var items)
                        && items.TryGetValue("CurrentUserId", out var currentUserIdObj)
                        && currentUserIdObj is int currentUserId
                        && src.Followers.Any(f => f.FollowerId == currentUserId)))
                .ForMember(dest => dest.IsConnectionRequestedByCurrentInvestor,
                    opt => opt.MapFrom((src, _, _, context) =>
                        context.TryGetItems(out var items)
                        && items.TryGetValue("CurrentInvestorId", out var currentInvestorIdObj)
                        && currentInvestorIdObj is int currentInvestorId
                        && src.ConnectionRequests.Any(cr => cr.InvestorId == currentInvestorId)))
                .ForMember(dest => dest.AssignedAdvisorId,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => (int?)pa.AdvisorId)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorName,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null && pa.Advisor.User != null ? pa.Advisor.User.UserName : null)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorHourlyRate,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null ? pa.Advisor.HourlyRate : null)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorRating,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null ? pa.Advisor.Rating : null)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorIndustries,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null && pa.Advisor.AdvisorIndustries != null
                            ? pa.Advisor.AdvisorIndustries.Select(ai => ai.IndustryOption.Value).ToList()
                            : new List<string>())
                        .FirstOrDefault() ?? new List<string>()));

            // Project Entity ? NonPremiumProjectResponse
            CreateMap<Project, NonPremiumProjectResponse>()
                .ForMember(dest => dest.StartupId,
                    opt => opt.MapFrom(src => src.StartupId))
                .ForMember(dest => dest.StageOptionId,
                    opt => opt.MapFrom(src => src.StageOptionId))
                .ForMember(dest => dest.Industries,
                    opt => opt.MapFrom(src => src.ProjectIndustries.Select(pi => pi.IndustryOption.Value).ToList()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.StartupPotentialScore,
                    opt => opt.MapFrom(src => src.StartupAIAnalysis != null ? src.StartupAIAnalysis.PotentialScore : null))
                .ForMember(dest => dest.FollowerCount,
                    opt => opt.MapFrom(src => src.Followers.Count))
                .ForMember(dest => dest.IsFollowedByCurrentUser,
                    opt => opt.MapFrom((src, _, _, context) =>
                        context.TryGetItems(out var items)
                        && items.TryGetValue("CurrentUserId", out var currentUserIdObj)
                        && currentUserIdObj is int currentUserId
                        && src.Followers.Any(f => f.FollowerId == currentUserId)))
                .ForMember(dest => dest.IsConnectionRequestedByCurrentInvestor,
                    opt => opt.MapFrom((src, _, _, context) =>
                        context.TryGetItems(out var items)
                        && items.TryGetValue("CurrentInvestorId", out var currentInvestorIdObj)
                        && currentInvestorIdObj is int currentInvestorId
                        && src.ConnectionRequests.Any(cr => cr.InvestorId == currentInvestorId)))
                .ForMember(dest => dest.AssignedAdvisorId,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => (int?)pa.AdvisorId)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorName,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null && pa.Advisor.User != null ? pa.Advisor.User.UserName : null)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorHourlyRate,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null ? pa.Advisor.HourlyRate : null)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorRating,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null ? pa.Advisor.Rating : null)
                        .FirstOrDefault()))
                .ForMember(dest => dest.AssignedAdvisorIndustries,
                    opt => opt.MapFrom(src => src.ProjectAdvisorAssignments
                        .OrderByDescending(pa => pa.AssignedAt)
                        .Select(pa => pa.Advisor != null && pa.Advisor.AdvisorIndustries != null
                            ? pa.Advisor.AdvisorIndustries.Select(ai => ai.IndustryOption.Value).ToList()
                            : new List<string>())
                        .FirstOrDefault() ?? new List<string>()));

            // CreateProjectRequest -> Project Entity
            CreateMap<CreateProjectRequest, Project>()
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.ProjectName == null ? string.Empty : src.ProjectName.Trim()))
                .ForMember(dest => dest.ShortDescription,
                    opt => opt.MapFrom(src => src.ShortDescription == null ? null : src.ShortDescription.Trim()))
                .ForMember(dest => dest.ProblemStatement,
                    opt => opt.MapFrom(src => src.ProblemStatement == null ? null : src.ProblemStatement.Trim()))
                .ForMember(dest => dest.SolutionDescription,
                    opt => opt.MapFrom(src => src.SolutionDescription == null ? null : src.SolutionDescription.Trim()))
                .ForMember(dest => dest.TargetCustomers,
                    opt => opt.MapFrom(src => src.TargetCustomers == null ? null : src.TargetCustomers.Trim()))
                .ForMember(dest => dest.UniqueValueProposition,
                    opt => opt.MapFrom(src => src.UniqueValueProposition == null ? null : src.UniqueValueProposition.Trim()))
                .ForMember(dest => dest.BusinessModel,
                    opt => opt.MapFrom(src => src.BusinessModel == null ? null : src.BusinessModel.Trim()))
                .ForMember(dest => dest.Competitors,
                    opt => opt.MapFrom(src => src.Competitors == null ? null : src.Competitors.Trim()))
                .ForMember(dest => dest.TeamMembers,
                    opt => opt.MapFrom(src => src.TeamMembers == null ? null : src.TeamMembers.Trim()))
                .ForMember(dest => dest.KeySkills,
                    opt => opt.MapFrom(src => src.KeySkills == null ? null : src.KeySkills.Trim()))
                .ForMember(dest => dest.TeamExperience,
                    opt => opt.MapFrom(src => src.TeamExperience == null ? null : src.TeamExperience.Trim()))
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
                .ForMember(dest => dest.StageOption, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectIndustries, opt => opt.Ignore())
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
                .ForMember(dest => dest.StageOption, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectIndustries, opt => opt.Ignore())
                .ForMember(dest => dest.UnlockedProjects, opt => opt.Ignore())
                .ForMember(dest => dest.ConnectionRequests, opt => opt.Ignore())
                .ForMember(dest => dest.Deals, opt => opt.Ignore());

            updateProjectMap.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // StartupAIAnalysis Entity ? StartupAIAnalysisResponse
            CreateMap<StartupAIAnalysis, StartupAIAnalysisResponse>()
                .ForMember(dest => dest.Analysis, opt => opt.Ignore())
                .ForMember(dest => dest.ScoreBreakdown, opt => opt.Ignore());

            // InvestorAIAnalysis Entity ? InvestorAIAnalysisResponse
            CreateMap<InvestorAIAnalysis, InvestorAIAnalysisResponse>()
                .ForMember(dest => dest.Analysis, opt => opt.Ignore())
                .ForMember(dest => dest.PotentialScore, opt => opt.Ignore())
                .ForMember(dest => dest.ScoreBreakdown, opt => opt.Ignore());

            // StartupAIAnalysis Entity -> StartupEligibilityResponse
            CreateMap<StartupAIAnalysis, StartupEligibilityResponse>()
                .ForMember(dest => dest.IsEligibleStartup,
                    opt => opt.MapFrom(src => src.IsEligibleStartup ?? false))
                .ForMember(dest => dest.EligibilityReason,
                    opt => opt.MapFrom(src => src.EligibilityReason ?? string.Empty));

            // Notification Entity -> NotificationDto
            CreateMap<Notification, NotificationDto>();

            // Deal Entity -> DealDto
            CreateMap<Deal, DealDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.InitiatorRole,
                    opt => opt.MapFrom(src => src.InitiatorRole.ToString()))
                .ForMember(dest => dest.StartupId,
                    opt => opt.MapFrom(src => src.Project != null ? src.Project.StartupId : 0))
                .ForMember(dest => dest.InvestorName,
                    opt => opt.MapFrom(src => src.Investor != null
                        ? (!string.IsNullOrWhiteSpace(src.Investor.OrganizationName)
                            ? src.Investor.OrganizationName
                            : (src.Investor.User != null
                                ? (src.Investor.User.UserName ?? string.Empty)
                                : string.Empty))
                        : string.Empty))
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Project != null
                        ? (src.Project.ProjectName ?? string.Empty)
                        : string.Empty))
                .ForMember(dest => dest.StartupName,
                    opt => opt.MapFrom(src => src.Project != null
                        && src.Project.Startup != null
                        ? (!string.IsNullOrWhiteSpace(src.Project.Startup.CompanyName)
                            ? src.Project.Startup.CompanyName
                            : (src.Project.Startup.User != null
                                ? (src.Project.Startup.User.UserName ?? string.Empty)
                                : string.Empty))
                        : string.Empty));

            // CreateDealDto -> Deal Entity
            CreateMap<CreateDealDto, Deal>()
                .ForMember(dest => dest.DealId, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorId, opt => opt.Ignore())
                .ForMember(dest => dest.StartupConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.InvestorConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.DealDate, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentUrl, opt => opt.Ignore())
                .ForMember(dest => dest.InitiatorRole, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
                .ForMember(dest => dest.CompletionDate, opt => opt.Ignore())
                .ForMember(dest => dest.Investor, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore());

            // CreatePostPrRequest -> PostPr Entity
            CreateMap<CreatePostPrRequest, PostPr>()
                .ForMember(dest => dest.PostPrId, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.IsDelete, opt => opt.Ignore())
                .ForMember(dest => dest.PublishedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Deal, opt => opt.Ignore());

            // PostPr Entity -> PostPrResponseDto
            CreateMap<PostPr, PostPrResponseDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ProjectId,
                    opt => opt.MapFrom(src => src.Deal != null ? src.Deal.ProjectId : 0))
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Deal != null && src.Deal.Project != null
                        ? src.Deal.Project.ProjectName
                        : string.Empty))
                .ForMember(dest => dest.ProjectImage,
                    opt => opt.MapFrom(src => src.Deal != null && src.Deal.Project != null
                        ? src.Deal.Project.ProjectImageUrl
                        : null))
                .ForMember(dest => dest.InvestorId,
                    opt => opt.MapFrom(src => src.Deal != null ? src.Deal.InvestorId : 0))
                .ForMember(dest => dest.InvestorName,
                    opt => opt.MapFrom(src => src.Deal != null && src.Deal.Investor != null
                        ? (!string.IsNullOrWhiteSpace(src.Deal.Investor.OrganizationName)
                            ? src.Deal.Investor.OrganizationName
                            : (src.Deal.Investor.User != null
                                ? (src.Deal.Investor.User.UserName ?? string.Empty)
                                : string.Empty))
                        : string.Empty))
                .ForMember(dest => dest.StartupId,
                    opt => opt.MapFrom(src => src.Deal != null && src.Deal.Project != null
                        ? src.Deal.Project.StartupId
                        : 0))
                .ForMember(dest => dest.StartupName,
                    opt => opt.MapFrom(src => src.Deal != null && src.Deal.Project != null && src.Deal.Project.Startup != null
                        ? (!string.IsNullOrWhiteSpace(src.Deal.Project.Startup.CompanyName)
                            ? src.Deal.Project.Startup.CompanyName
                            : (src.Deal.Project.Startup.User != null
                                ? (src.Deal.Project.Startup.User.UserName ?? string.Empty)
                                : string.Empty))
                        : string.Empty));

            // ConnectionRequest Entity -> ConnectionRequestDto
            CreateMap<ConnectionRequest, ConnectionRequestDto>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.InvestorName,
                    opt => opt.MapFrom(src => src.Investor != null && src.Investor.User != null
                        ? (src.Investor.User.UserName ?? string.Empty)
                        : string.Empty))
                .ForMember(dest => dest.ProjectName,
                    opt => opt.MapFrom(src => src.Project != null
                        ? (src.Project.ProjectName ?? string.Empty)
                        : string.Empty))
                .ForMember(dest => dest.StartupName,
                    opt => opt.MapFrom(src => src.Project != null
                        && src.Project.Startup != null
                        && src.Project.Startup.User != null
                        ? (src.Project.Startup.User.UserName ?? string.Empty)
                        : string.Empty))
                .ForMember(dest => dest.ChatSessionId,
                    opt => opt.MapFrom(src => src.ChatSession != null ? (int?)src.ChatSession.ChatSessionId : null));

            // Startup Entity -> ContactInfoDto
            CreateMap<Startup, ContactInfoDto>();

            // CreateUserReportRequest -> UserReport Entity
            CreateMap<CreateUserReportRequest, UserReport>()
                .ForMember(dest => dest.UserReportId, opt => opt.Ignore())
                .ForMember(dest => dest.ReporterId, opt => opt.Ignore())
                .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.EvidenceUrl, opt => opt.Ignore())
                .ForMember(dest => dest.EvidenceImageUrls, opt => opt.Ignore())
                .ForMember(dest => dest.VideoEvidenceUrl, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.VideoEvidenceUrl) ? null : src.VideoEvidenceUrl.Trim()))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ResolutionNote, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedById, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore())
                .ForMember(dest => dest.Reporter, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedBy, opt => opt.Ignore());

            // UserReport Entity -> UserReportResponse
            CreateMap<UserReport, UserReportResponse>()
                .ForMember(dest => dest.Category,
                    opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => src.Reason))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.EvidenceImageUrls,
                    opt => opt.MapFrom(src => ParseEvidenceImageUrls(src.EvidenceImageUrls, src.EvidenceUrl)));

            CreateMap<Wallet, WalletSummaryResponse>()
                ;

            CreateMap<Wallet, AdvisorWalletResponse>()
                .ForMember(dest => dest.AdvisorUserId,
                    opt => opt.MapFrom(src => src.Advisor.UserId))
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Advisor.User != null
                        ? (src.Advisor.User.UserName ?? $"Advisor {src.AdvisorId}")
                        : $"Advisor {src.AdvisorId}"))
                .ForMember(dest => dest.AdvisorEmail,
                    opt => opt.MapFrom(src => src.Advisor.User != null
                        ? (src.Advisor.User.Email ?? string.Empty)
                        : string.Empty));

            CreateMap<WalletTransaction, WalletTransactionResponse>()
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Payout, PayoutResponse>()
                .ForMember(dest => dest.AdvisorId,
                    opt => opt.MapFrom(src => src.Wallet.AdvisorId))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Wallet.Advisor != null && src.Wallet.Advisor.User != null
                        ? (src.Wallet.Advisor.User.UserName ?? $"Advisor {src.Wallet.AdvisorId}")
                        : $"Advisor {src.Wallet.AdvisorId}"))
                .ForMember(dest => dest.PaidByName,
                    opt => opt.MapFrom(src => src.PaidBy != null
                        ? (src.PaidBy.UserName ?? string.Empty)
                        : null))
                .ForMember(dest => dest.RejectedByName,
                    opt => opt.MapFrom(src => src.RejectedBy != null
                        ? (src.RejectedBy.UserName ?? string.Empty)
                        : null));

            CreateMap<PayoutGroup, PayoutGroupResponse>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.TotalBillCount, opt => opt.MapFrom(src => src.Payouts.Count))
                .ForMember(dest => dest.PendingBillCount, opt => opt.MapFrom(src => src.Payouts.Count(x =>
                    x.Status == MonthlyPayoutStatus.Pending || x.Status == MonthlyPayoutStatus.PendingRecheck)))
                .ForMember(dest => dest.ApprovedBillCount, opt => opt.MapFrom(src => src.Payouts.Count(x => x.Status == MonthlyPayoutStatus.Paid)))
                .ForMember(dest => dest.RejectedBillCount, opt => opt.MapFrom(src => src.Payouts.Count(x => x.Status == MonthlyPayoutStatus.Rejected)));

            CreateMap<AdvisorBankAccount, AdvisorBankAccountResponse>()
                .ForMember(dest => dest.AdvisorName,
                    opt => opt.MapFrom(src => src.Advisor.User != null
                        ? (src.Advisor.User.UserName ?? $"Advisor {src.AdvisorId}")
                        : $"Advisor {src.AdvisorId}"));

        }

        private static List<string> ParseEvidenceImageUrls(string? evidenceImageUrlsJson, string? legacyEvidenceUrl)
        {
            if (!string.IsNullOrWhiteSpace(evidenceImageUrlsJson))
            {
                try
                {
                    var urls = JsonSerializer.Deserialize<List<string>>(evidenceImageUrlsJson);
                    if (urls is not null && urls.Count > 0)
                    {
                        return urls;
                    }
                }
                catch
                {
                    // fallback to legacy url
                }
            }

            return string.IsNullOrWhiteSpace(legacyEvidenceUrl)
                ? []
                : [legacyEvidenceUrl];
        }
    }
}





