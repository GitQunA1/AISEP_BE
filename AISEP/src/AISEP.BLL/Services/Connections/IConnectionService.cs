using AISEP.BLL.DTOs.Requests;
using AISEP.BLL.DTOs.Responses;

namespace AISEP.BLL.Services.Connections
{
    public interface IConnectionService
    {
        Task<ConnectionRequestDto> CreateRequestAsync(int investorId, CreateConnectionRequestDto dto);
        Task<ConnectionRequestDto> RespondToRequestAsync(int startupId, int requestId, bool isAccepted);
        Task<ContactInfoDto> GetFounderContactAsync(int investorId, int projectId);
    }
}
