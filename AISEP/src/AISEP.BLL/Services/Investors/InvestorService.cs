using AISEP.BLL.Common;
using AISEP.DAL.Common;
using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Entities;
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

        public async Task<InvestorResponse?> CreateAsync(int userId, InvestorRequest dto)
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

        public async Task<InvestorResponse?> UpdateAsync(int userId, InvestorRequest dto)
        {
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (investor is null)
                return null;

            _mapper.Map(dto, investor);

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.Investors.SaveChangesAsync();

            return _mapper.Map<InvestorResponse>(investor);
        }
    }
}

