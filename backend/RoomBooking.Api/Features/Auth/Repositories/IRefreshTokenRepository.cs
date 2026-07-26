using RoomBooking.Api.Features.Auth.Entities;

namespace RoomBooking.Api.Features.Auth.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindActiveByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
