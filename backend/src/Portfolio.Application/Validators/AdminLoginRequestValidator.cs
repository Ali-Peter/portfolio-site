using FluentValidation;
using Portfolio.Application.Dto;

namespace Portfolio.Application.Validators;

/// <summary>Rules for the admin login payload (generic — no credential hints).</summary>
public sealed class AdminLoginRequestValidator : AbstractValidator<AdminLoginRequest>
{
    public AdminLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(320).WithMessage("Email must be at most 320 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(200).WithMessage("Password must be at most 200 characters.");
    }
}
