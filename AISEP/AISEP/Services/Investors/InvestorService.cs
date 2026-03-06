using AISEP.Common;
using AISEP.DTOs;
using AISEP.Models.Entities;
using AISEP.Repositories.Investors;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Investors
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

        public async Task<PagedResultDto<InvestorResponseDto>> GetAllAsync(SieveModel model)
        {
            var query = _unitOfWork.Investors.GetAllQuery();

            var totalCount = await _sieveProcessor
                .Apply(model, query, applyPagination: false, applySorting: false)
                .CountAsync();

            var items = await _sieveProcessor
                .Apply(model, query)
                .ToListAsync();

            var page = model.Page ?? 1;
            var pageSize = model.PageSize ?? 10;

            return new PagedResultDto<InvestorResponseDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items.Select(i => _mapper.Map<InvestorResponseDto>(i))
            };
        }

        public async Task<InvestorResponseDto?> GetByIdAsync(int investorId)
        {
            var investor = await _unitOfWork.Investors.GetByIdAsync(investorId);
            return investor is null ? null : _mapper.Map<InvestorResponseDto>(investor);
        }

        public async Task<InvestorResponseDto?> GetMyProfileAsync(int userId)
        {
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            return investor is null ? null : _mapper.Map<InvestorResponseDto>(investor);
        }

        public async Task<InvestorResponseDto?> CreateAsync(int userId, InvestorDto dto)
        {
            var existing = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (existing is not null)
                return null;

            var investor = _mapper.Map<Investor>(dto);
            investor.UserId = userId;

            await _unitOfWork.Investors.AddAsync(investor);
            await _unitOfWork.Investors.SaveChangesAsync();

            var created = await _unitOfWork.Investors.GetByIdAsync(investor.InvestorId);
            return _mapper.Map<InvestorResponseDto>(created!);
        }

        public async Task<InvestorResponseDto?> UpdateAsync(int userId, InvestorDto dto)
        {
            var investor = await _unitOfWork.Investors.GetByUserIdAsync(userId);
            if (investor is null)
                return null;

            _mapper.Map(dto, investor);

            _unitOfWork.Investors.Update(investor);
            await _unitOfWork.Investors.SaveChangesAsync();

            return _mapper.Map<InvestorResponseDto>(investor);
        }
    }
}
