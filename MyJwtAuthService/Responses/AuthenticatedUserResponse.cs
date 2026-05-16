namespace MyJwtAuthService.Responses
{
    public class AuthenticatedUserResponse
    {
        public required string AccessToken { get; set; }
        public required DateTime AccessTokenExpirationTime { get; set; }
        public required string RefreshToken { get; set; }
    }
}
