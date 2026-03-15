using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AISEP.DAL.Repositories.Investors;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;
using AISEP.BLL.Services.Users;

namespace AISEP.BLL.Services.Investors
{
    public class InvestorService : IInvestorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public InvestorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<PagedResult<InvestorResponse>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Investors.GetAllQuery();
            return await PaginationHelper.PaginateAsync(query, model, _sieveProcessor, i => _mapper.Map<InvestorResponse>(i));
        }

        public async Task<InvestorResponse?> GetByIdAsync(int investorId)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            return investor is null ? null : _mapper.Map<InvestorResponse>(investor);
        }

        public async Task<InvestorResponse?> GetMyProfileAsync(int userId)
        {
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            return investor is null ? null : _mapper.Map<InvestorResponse>(investor);
           
        }

        public async Task<InvestorResponse?> CreateAsync( CreateInvestorRequest dto)
        {   var userId = _userService.GetUserId();
            var existing = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (existing is not null)
                return null;

            var investor = _mapper.Map<Investor>(dto);
            investor.UserId = userId;
           // investor.CreatedAt = DateTime.UtcNow;
            investor.CreatedBy = userId;
            investor.ApprovalStatus = ApprovalStatus.Pending;
            await _unitOfWork.Investors.AddAsync(investor);
            await _unitOfWork.Investors.SaveChangesAsync();

            var created = await _unitOfWork.Investors.GetByIdAsync(investor.InvestorId);
            return _mapper.Map<InvestorResponse>(created!);
        }

        public async Task<InvestorResponse?> UpdateAsync(int id, UpdateInvestorRequest dto)
        {
            var userId = _userService.GetUserId();

            var investor = await _unitOfWork.Investors.GetByIdAsync(id);
            if (investor is null)
                return null;
            if (investor.CreatedBy != userId)
                throw new UnauthorizedAccessException("You are not authorized to update this investor.");
            investor.OrganizationName    = dto.OrganizationName    ?? investor.OrganizationName;
            investor.InvestmentTaste     = dto.InvestmentTaste     ?? investor.InvestmentTaste;
            investor.WalletAddress       = dto.WalletAddress       ?? investor.WalletAddress;
            investor.InvestmentAmount    = (dto.InvestmentAmount > 0) ? dto.InvestmentAmount : investor.InvestmentAmount;
            investor.InvestmentDate      = dto.InvestmentDate      ?? investor.InvestmentDate;
            investor.RiskTolerance       = dto.RiskTolerance       ?? investor.RiskTolerance;
            investor.InvestmentRegion    = dto.InvestmentRegion    ?? investor.InvestmentRegion;
            investor.FocusIndustry       = dto.FocusIndustry       ?? investor.FocusIndustry;
            investor.PreferredStage      = dto.PreferredStage      ?? investor.PreferredStage;
            investor.PreviousInvestments = dto.PreviousInvestments ?? investor.PreviousInvestments;

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.Investors.SaveChangesAsync();

            return _mapper.Map<InvestorResponse>(investor);
        }

        public async Task ApproveInvestorAsync(int investorId)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            if (investor is null)
                throw new KeyNotFoundException("Investor not found.");
            if (investor.ApprovalStatus == ApprovalStatus.Approved)
                throw new InvalidOperationException("Investor is already approved.");
            if (investor.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending investors can be approved. Current status: {investor.ApprovalStatus}.");

            investor.ApprovalStatus = ApprovalStatus.Approved;
            investor.ApprovedAt     = DateTime.UtcNow;
            investor.ApprovedById   = userId;

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RejectInvestorAsync(int investorId, string rejectionReason)
        {
            var userId = _userService.GetUserId();
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            if (investor is null)
                throw new KeyNotFoundException("Investor not found.");
            if (investor.ApprovalStatus == ApprovalStatus.Rejected)
                throw new InvalidOperationException("Investor is already rejected.");
            if (investor.ApprovalStatus != ApprovalStatus.Pending)
                throw new InvalidOperationException($"Only Pending investors can be rejected. Current status: {investor.ApprovalStatus}.");

            investor.ApprovalStatus  = ApprovalStatus.Rejected;
            investor.RejectedAt      = DateTime.UtcNow;
            investor.RejectedById    = userId;
            investor.RejectionReason = rejectionReason;

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

