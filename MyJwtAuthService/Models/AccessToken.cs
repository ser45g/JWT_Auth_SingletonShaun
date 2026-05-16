namespace MyJwtAuthService.Models
{
    public class AccessToken
    {
        public required string Value { get; set; }
        public required DateTime ExpirationTime { get; set; }
    }
}
