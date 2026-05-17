using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class RefreshRequestValidator:AbstractValidator<RefreshRequest>
    {
        public RefreshRequestValidator() {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
