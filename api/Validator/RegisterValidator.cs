using api.Dto;
using FluentValidation;

namespace api.Validator
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator() 
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Your email cannot be empty.")
                .EmailAddress().WithMessage("Your email is not a valid email.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Your password cannot be empty.")
                .MinimumLength(8).WithMessage("Your password length must be at least 8.")
                .MaximumLength(18).WithMessage("Your password length must not exceed 16.")
                .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.")
                .Matches(@"[\!\?\*\.]+").WithMessage("Your password must contain at least one special character between (! ? *.).");
        }
    }
}
