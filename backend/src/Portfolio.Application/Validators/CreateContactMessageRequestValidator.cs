using FluentValidation;
using Portfolio.Application.Dto;

namespace Portfolio.Application.Validators;

/// <summary>
/// Rules for public contact submissions.
/// The honeypot field (<c>Website</c>) is intentionally NOT validated here —
/// bots are silently accepted by the service, never rejected.
/// </summary>
public sealed class CreateContactMessageRequestValidator : AbstractValidator<CreateContactMessageRequest>
{
    public CreateContactMessageRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(320).WithMessage("Email must be at most 320 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MinimumLength(10).WithMessage("Message must be at least 10 characters.")
            .MaximumLength(5000).WithMessage("Message must be at most 5000 characters.");
    }
}
