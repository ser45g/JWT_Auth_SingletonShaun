using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class ResetPasswordRequestValidator:AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator() { 
            RuleFor(r=>r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.NewPassword).NotEmpty().MaximumLength(200);
            RuleFor(r => r.ResetCode).NotEmpty();
        }
    }
}
