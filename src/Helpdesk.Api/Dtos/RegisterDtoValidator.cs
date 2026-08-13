using FluentValidation;

namespace Helpdesk.Api.Dtos
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);

            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

            RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        }
    }
}
