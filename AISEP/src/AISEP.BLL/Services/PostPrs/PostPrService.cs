using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AISEP.DAL.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.PostPrs
{
    public class PostPrService : IPostPrService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ISieveProcessor _sieveProcessor;

        public PostPrService(IUnitOfWork unitOfWork, IMapper mapper, ISieveProcessor sieveProcessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _sieveProcessor = sieveProcessor;
        }

        public async Task<PagedResult<PostPrResponseDto>> GetListAsync(SieveModel sieveModel)
        {
            var query = BuildQuery();
            return await PaginationHelper.PaginateAsync(query, sieveModel, _sieveProcessor, p => _mapper.Map<PostPrResponseDto>(p));
        }

        public async Task<PostPrResponseDto> GetByIdAsync(int id)
        {
            var postPr = await BuildQuery().FirstOrDefaultAsync(p => p.PostPrId == id)
                ?? throw new KeyNotFoundException("Post PR not found.");

            return _mapper.Map<PostPrResponseDto>(postPr);
        }

        public async Task<PostPrResponseDto> CreateAsync(CreatePostPrRequest request)
        {
            var deal = await _unitOfWork.Deals.GetByIdAsync(request.DealId);
            if (deal is null)
            {
                throw new KeyNotFoundException("Deal not found.");
            }

            if (deal.Status != DealStatus.Contract_Signed)
            {
                throw new InvalidOperationException("Only deals with status Contract_Signed can create Post PR.");
            }

            var postPr = _mapper.Map<PostPr>(request);
            postPr.Status = PostPrStatus.Pending;
            postPr.IsDelete = false;
            postPr.PublishedAt = null;

            await _unitOfWork.PostPrs.AddAsync(postPr);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(postPr.PostPrId);
        }

        public async Task<PostPrResponseDto> UpdateAsync(int id, UpdatePostPrRequest request)
        {
            var postPr = await BuildQuery().FirstOrDefaultAsync(p => p.PostPrId == id)
                ?? throw new KeyNotFoundException("Post PR not found.");

            if (request.Title is not null)
            {
                postPr.Title = request.Title.Trim();
            }

            if (request.Content is not null)
            {
                postPr.Content = request.Content.Trim();
            }

            _unitOfWork.PostPrs.Update(postPr);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(postPr.PostPrId);
        }

        public async Task<PostPrResponseDto> PatchPublishAsync(int id)
        {
            var postPr = await BuildQuery().FirstOrDefaultAsync(p => p.PostPrId == id)
                ?? throw new KeyNotFoundException("Post PR not found.");

            if (postPr.Status != PostPrStatus.Public)
            {
                postPr.Status = PostPrStatus.Public;
                postPr.PublishedAt = postPr.PublishedAt ?? DateTime.UtcNow;
                _unitOfWork.PostPrs.Update(postPr);
                await _unitOfWork.SaveChangesAsync();
            }

            return await GetByIdAsync(postPr.PostPrId);
        }

        public async Task PatchDeleteAsync(int id, bool isDelete)
        {
            var postPr = await _unitOfWork.PostPrs.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Post PR not found.");

            postPr.IsDelete = isDelete;
            _unitOfWork.PostPrs.Update(postPr);
            await _unitOfWork.SaveChangesAsync();
        }

        private IQueryable<PostPr> BuildQuery()
        {
            return _unitOfWork.PostPrs.GetQuery()
                .Where(p => !p.IsDelete)
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Project)
                        .ThenInclude(project => project.Startup)
                            .ThenInclude(startup => startup.User)
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Investor)
                        .ThenInclude(investor => investor.User)
                .OrderByDescending(p => p.PostPrId)
                .AsQueryable();
        }
    }
}
