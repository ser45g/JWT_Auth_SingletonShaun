using MyJwtAuthService.Models;

namespace MyJwtAuthService.Services.RefreshTokenRepositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByToken(string token);

        Task Create(RefreshToken refreshToken);

        Task Delete(Guid id);

        Task DeleteAll(Guid userId);
    }
}
