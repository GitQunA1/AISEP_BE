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
        private readonly IInvestorRepository _repository;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public InvestorService(IInvestorRepository repository, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _repository = repository;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<InvestorResponseDto>> GetAllAsync(SieveModel model)
        {
            var query = _repository.GetAllQuery();

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
            var investor = await _repository.GetByIdAsync(investorId);
            return investor is null ? null : _mapper.Map<InvestorResponseDto>(investor);
        }

        public async Task<InvestorResponseDto?> GetMyProfileAsync(int userId)
        {
            var investor = await _repository.GetByUserIdAsync(userId);
            return investor is null ? null : _mapper.Map<InvestorResponseDto>(investor);
        }

        public async Task<InvestorResponseDto?> CreateAsync(int userId, InvestorDto dto)
        {
            var existing = await _repository.GetByUserIdAsync(userId);
            if (existing is not null)
                return null;

            var investor = _mapper.Map<Investor>(dto);
            investor.UserId = userId;

            await _repository.AddAsync(investor);
            await _repository.SaveChangesAsync();

            var created = await _repository.GetByIdAsync(investor.InvestorId);
            return _mapper.Map<InvestorResponseDto>(created!);
        }

        public async Task<InvestorResponseDto?> UpdateAsync(int userId, InvestorDto dto)
        {
            var investor = await _repository.GetByUserIdAsync(userId);
            if (investor is null)
                return null;

            _mapper.Map(dto, investor);

            _repository.Update(investor);
            await _repository.SaveChangesAsync();

            return _mapper.Map<InvestorResponseDto>(investor);
        }
    }
}
