using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class ChangeEmailRequestValidator:AbstractValidator<ChangeEmailRequest>
    {
        public ChangeEmailRequestValidator() {
            RuleFor(c => c.NewEmail).NotEmpty().EmailAddress();
        }
    }
}
