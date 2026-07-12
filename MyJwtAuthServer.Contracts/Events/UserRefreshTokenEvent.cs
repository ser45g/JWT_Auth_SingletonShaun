
namespace MyJwtAuthServer.Contracts.Events
{
    public record class UserRefreshTokenEvent(string Username, string Email, string Token, string NewToken, DateTime OccuredOnUtc);


}
