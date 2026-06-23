namespace MyJwtAuthService.Responses
{
    public record class UserInfoResponse(Guid Id, string? Username, string? Email, bool EmailConfirmed, IEnumerable<string> Roles);
}
