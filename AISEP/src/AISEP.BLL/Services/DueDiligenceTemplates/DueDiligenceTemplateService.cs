using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using AISEP.DAL.Common;
using AISEP.DAL.Entities;
using AutoMapper;

namespace AISEP.BLL.Services.DueDiligenceTemplates
{
    public class DueDiligenceTemplateService : IDueDiligenceTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DueDiligenceTemplateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<DueDiligenceTemplateResponse> GetAsync()
        {
            var template = await _unitOfWork.DueDiligenceTemplates.GetAsync()
                ?? throw new KeyNotFoundException("Due diligence template not found.");

            return _mapper.Map<DueDiligenceTemplateResponse>(template);
        }

        public async Task<DueDiligenceTemplateResponse> UpsertAsync(UpsertDueDiligenceTemplateRequest request)
        {
            var contentJson = request.ContentJson?.Trim() ?? string.Empty;

            var template = await _unitOfWork.DueDiligenceTemplates.GetAsync();
            if (template is null)
            {
                template = new DueDiligenceTemplate
                {
                    ContentJson = contentJson,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.DueDiligenceTemplates.AddAsync(template);
            }
            else
            {
                template.ContentJson = contentJson;
                template.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.DueDiligenceTemplates.Update(template);
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<DueDiligenceTemplateResponse>(template);
        }
    }
}
