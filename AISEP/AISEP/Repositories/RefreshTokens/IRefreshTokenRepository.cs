using AISEP.Models.Entities;
using AISEP.Models;


namespace AISEP.Repositories.RefreshTokens
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
        Task AddAsync(RefreshToken refreshToken);
        Task UpdateAsync(RefreshToken refreshToken);
        Task UpdateRangeAsync(IEnumerable<RefreshToken> refreshTokens);
    }
}
