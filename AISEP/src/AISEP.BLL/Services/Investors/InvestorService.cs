using AISEP.BLL.Common;
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

namespace AISEP.BLL.Services.Investors
{
    public class InvestorService : IInvestorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public InvestorService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
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

        public async Task<InvestorResponse?> CreateAsync(int userId, CreateInvestorRequest dto)
        {
            var existing = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (existing is not null)
                return null;

            var investor = _mapper.Map<Investor>(dto);
            investor.UserId = userId;

            await _unitOfWork.Investors.AddAsync(investor);
            await _unitOfWork.Investors.SaveChangesAsync();

            var created = await _unitOfWork.Investors.GetByIdAsync(investor.InvestorId);
            return _mapper.Map<InvestorResponse>(created!);
        }

        public async Task<InvestorResponse?> UpdateAsync(int userId, UpdateInvestorRequest dto)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(userId);
            if (investor is null)
                return null;

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
    }
}

