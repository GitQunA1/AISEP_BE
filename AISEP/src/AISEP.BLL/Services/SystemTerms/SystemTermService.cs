using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.BLL.Helpers;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;
using Sieve.Models;
using Sieve.Services;

namespace AISEP.BLL.Services.SystemTerms
{
    public class SystemTermService : ISystemTermService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISieveProcessor _sieveProcessor;
        private readonly IMapper _mapper;

        public SystemTermService(IUnitOfWork unitOfWork, ISieveProcessor sieveProcessor, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _sieveProcessor = sieveProcessor;
            _mapper = mapper;
        }

        public async Task<SystemTermResponse> PublishAsync(CreateSystemTermRequest request)
        {
            var activeTerm = await _unitOfWork.SystemTerms.GetActiveAsync();
            if (activeTerm is not null)
            {
                activeTerm.IsActive = false;
                _unitOfWork.SystemTerms.Update(activeTerm);
            }

            var systemTerm = new SystemTerm
            {
                ContentHtml = request.ContentHtml.Trim(),
                Version = request.Version.Trim(),
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.SystemTerms.AddAsync(systemTerm);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<SystemTermResponse>(systemTerm);
        }

        public async Task<SystemTermResponse> GetActiveAsync()
        {
            var activeTerm = await _unitOfWork.SystemTerms.GetActiveAsync()
                ?? throw new KeyNotFoundException("Active system terms not found.");

            return _mapper.Map<SystemTermResponse>(activeTerm);
        }

        public async Task<PagedResult<SystemTermResponse>> GetHistoryAsync(SieveModel model)
        {
            return await PaginationHelper.PaginateAsync(
                _unitOfWork.SystemTerms.GetQuery(),
                model,
                _sieveProcessor,
                x => _mapper.Map<SystemTermResponse>(x));
        }
    }
}
