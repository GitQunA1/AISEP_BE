using AISEP.DTOs;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using AISEP.Repositories.Startups;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Startups
{
    public class StartupService : IStartupService
    {
        private readonly IStartupRepository _repository;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public StartupService(IStartupRepository repository, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _repository = repository;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<StartupResponseDto>> SearchStartupsAsync(SieveModel model, string? industry = null, DevelopmentStage? stage = null)
        {
            var query = _repository.SearchStartupsQuery(industry, stage);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<StartupResponseDto?> GetStartupByIdAsync(int id)
        {
            var startup = await _repository.GetByIdAsync(id);
            return startup is null ? null : _mapper.Map<StartupResponseDto>(startup);
        }

        public async Task<PagedResultDto<StartupResponseDto>> GetAllStartupsAsync(SieveModel model)
        {
            var query = _repository.GetStartupQuery();
            return await ApplySieveAndPaginateAsync(query, model);
        }

        private async Task<PagedResultDto<StartupResponseDto>> ApplySieveAndPaginateAsync(
            IQueryable<Startup> query,
            SieveModel sieveModel)
        {
            var totalCount = await _sieveProcessor
                .Apply(sieveModel, query, applyPagination: false, applySorting: false)
                .CountAsync();

            var items = await _sieveProcessor
                .Apply(sieveModel, query)
                .ToListAsync();

            var page = sieveModel.Page ?? 1;
            var pageSize = sieveModel.PageSize ?? 10;

            return new PagedResultDto<StartupResponseDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items.Select(s => _mapper.Map<StartupResponseDto>(s))
            };
        }
    }
}
