using FluentValidation;
using Portfolio.Application.Dto;

namespace Portfolio.Application.Validators;

/// <summary>Input limits for assistant questions (public endpoint abuse control).</summary>
public sealed class CreateChatRequestValidator : AbstractValidator<CreateChatRequest>
{
    public const int MaxMessageLength = 2000;

    public CreateChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("A question is required.")
            .MaximumLength(MaxMessageLength)
            .WithMessage($"The question must be at most {MaxMessageLength} characters.");
    }
}