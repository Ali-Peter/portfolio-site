using FluentValidation;
using Portfolio.Application.Dto;

namespace Portfolio.Application.Validators;

/// <summary>Rules for admin skill create/update.</summary>
public sealed class UpsertSkillRequestValidator : AbstractValidator<UpsertSkillRequest>
{
    public UpsertSkillRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

        RuleFor(x => x.Category).IsInEnum().WithMessage("Category is invalid.");
        RuleFor(x => x.Proficiency).IsInEnum().WithMessage("Proficiency is invalid.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be 0 or greater.")
            .LessThanOrEqualTo(10000).WithMessage("Sort order must be at most 10000.");
    }
}

/// <summary>Rules for admin experience create/update.</summary>
public sealed class UpsertExperienceRequestValidator : AbstractValidator<UpsertExperienceRequest>
{
    public UpsertExperienceRequestValidator()
    {
        RuleFor(x => x.Company)
            .NotEmpty().WithMessage("Company is required.")
            .MaximumLength(200).WithMessage("Company must be at most 200 characters.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(20000).WithMessage("Description must be at most 20000 characters.");

        AdminValidationShared.TagList(RuleFor(x => x.Technologies), 30, 60);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be 0 or greater.")
            .LessThanOrEqualTo(10000).WithMessage("Sort order must be at most 10000.");

        RuleFor(x => x.EndedOn)
            .GreaterThanOrEqualTo(x => x.StartedOn)
            .When(x => x.StartedOn.HasValue && x.EndedOn.HasValue)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.EndedOn)
            .Empty().When(x => x.IsCurrent)
            .WithMessage("Current roles must not have an end date.");
    }
}
