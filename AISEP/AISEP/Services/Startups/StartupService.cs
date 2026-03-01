using AISEP.DTOs;
using AISEP.Models.Entities;
using AISEP.Models.Enums;
using AISEP.Repositories.Startups;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.Services.Startups
{
    public class StartupService : IStartupService
    {
        private readonly IStartupRepository _repository;
        private readonly ISieveProcessor _sieveProcessor;

        public StartupService(IStartupRepository repository, ISieveProcessor sieveProcessor)
        {
            _repository = repository;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<PagedResultDto<StartupResponseDto>> SearchStartupsAsync(SieveModel model, string? industry = null, DevelopmentStage? stage = null)
        {
            var query = _repository.SearchStartupsQuery(industry, stage);
            return await ApplySieveAndPaginateAsync(query, model);
        }

        public async Task<StartupResponseDto?> GetStartupByIdAsync(int id)
        {
            var startup = await _repository.GetByIdAsync(id);
            return startup != null ? MapToResponseDto(startup) : null;
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
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResultDto<StartupResponseDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items.Select(MapToResponseDto)
            };
        }

        private StartupResponseDto MapToResponseDto(Startup startup)
        {
            return new StartupResponseDto
            {
                Id = startup.StartupId,
                CompanyName = startup.CompanyName,
                LogoUrl = startup.LogoUrl,
                Founder = startup.Founder,
                CountryCity = startup.CountryCity,
                Website = startup.Website,
                Industry = startup.Industry,
                DevelopmentStage = startup.DevelopmentStage,
                ProblemStatement = startup.ProblemStatement,
                SolutionDescription = startup.SolutionDescription,
                MarketSize = startup.MarketSize,
                Revenue = startup.Revenue,
                FollowerCount = startup.Followers?.Count ?? 0
            };
        }
    }
}
