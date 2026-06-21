using MediatR;
using MyJwtAuthService.Models;

namespace MyJwtAuthService.Outbox.Messages
{
    public class PasswordResetLinkConfirmationOutboxMessage : INotification
    {
        public required ApplicationUser User { get; init; }
        public required string Email { get; init; }
        public required string ResetLink { get; init; }
    }
}
