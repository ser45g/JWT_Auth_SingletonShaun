using FluentValidation;
using MyJwtAuthService.Requests;

namespace MyJwtAuthService.Validators
{
    public class RegisterRequestValidator:AbstractValidator<RegisterRequest>
    {
        private readonly IValidator<LoginRequest> _loginRequestValidator;
        public RegisterRequestValidator(IValidator<LoginRequest> loginRequestValidator)
        {
            _loginRequestValidator = loginRequestValidator;
            RuleFor(r => r.Email).NotEmpty().EmailAddress();
            RuleFor(r => r.Password).NotEmpty().MaximumLength(200);
            RuleFor(r => r.ConfirmPassword).NotEmpty().Equal(r => r.Password);
            RuleFor(r => r.Username).NotEmpty().Length(4, 100);
        }
    }
}
