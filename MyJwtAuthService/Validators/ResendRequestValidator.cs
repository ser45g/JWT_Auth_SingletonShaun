using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class ResendRequestValidator:AbstractValidator<ResendRequest>
    {
        public ResendRequestValidator() {
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
        }
    }
}
