using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;
using Sieve.Models;

namespace AISEP.BLL.Services.PostPrs
{
    public interface IPostPrService
    {
        Task<PagedResult<PostPrResponseDto>> GetListAsync(SieveModel sieveModel);
        Task<PostPrResponseDto> GetByIdAsync(int id);
        Task<PostPrResponseDto> CreateAsync(CreatePostPrRequest request);
        Task<PostPrResponseDto> UpdateAsync(int id, UpdatePostPrRequest request);
        Task DeleteAsync(int id);
    }
}
