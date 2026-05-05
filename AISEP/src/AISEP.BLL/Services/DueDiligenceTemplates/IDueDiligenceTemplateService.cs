using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.DueDiligenceTemplates
{
    public interface IDueDiligenceTemplateService
    {
        Task<DueDiligenceTemplateResponse> GetAsync();
        Task<DueDiligenceTemplateResponse> UpsertAsync(UpsertDueDiligenceTemplateRequest request);
    }
}
