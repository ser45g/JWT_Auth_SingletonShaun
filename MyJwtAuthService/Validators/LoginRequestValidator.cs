using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class LoginRequestValidator: AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator() {
            RuleFor(l => l.Email).NotEmpty().EmailAddress();
            RuleFor(l => l.Password).NotEmpty().MaximumLength(200);
        }
    }
}
