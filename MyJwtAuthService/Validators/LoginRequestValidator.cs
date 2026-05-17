using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class LoginRequestValidator: AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator() {
            RuleFor(l => l.Username).NotEmpty().Length(4, 100);
            RuleFor(l => l.Password).NotEmpty().MaximumLength(200);
        }
    }
}
