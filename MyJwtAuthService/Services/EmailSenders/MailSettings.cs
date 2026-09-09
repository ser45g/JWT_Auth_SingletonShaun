using System.ComponentModel.DataAnnotations;

namespace MyJwtAuthService.Services.EmailSenders
{
    public class MailSettings
    {
        [Required]
        public required string From { get; set; }
        [Required]
        public required string DisplayName { get; set; }
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required string Host { get; set; }
        [Required]
        [Range(1, 100000)]
        public required int Port { get; set; }

        [Required]
        public required bool IsAuthenticated { get; set; }
    }
}
