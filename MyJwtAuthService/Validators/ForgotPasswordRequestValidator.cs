using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class ForgotPasswordRequestValidator:AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator() {
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
        }
    }
}
