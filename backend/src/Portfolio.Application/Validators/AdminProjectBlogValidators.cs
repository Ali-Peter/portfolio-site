using FluentValidation;
using Portfolio.Application.Dto;

namespace Portfolio.Application.Validators;

/// <summary>Rules for admin project create/update.</summary>
public sealed class UpsertProjectRequestValidator : AbstractValidator<UpsertProjectRequest>
{
    public UpsertProjectRequestValidator()
    {
        AdminValidationShared.Slug(RuleFor(x => x.Slug));

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary is required.")
            .MaximumLength(500).WithMessage("Summary must be at most 500 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(20000).WithMessage("Description must be at most 20000 characters.");

        AdminValidationShared.OptionalUrl(RuleFor(x => x.RepoUrl));
        AdminValidationShared.OptionalUrl(RuleFor(x => x.DemoUrl));
        AdminValidationShared.OptionalUrl(RuleFor(x => x.CoverImageUrl));

        RuleFor(x => x.Status).IsInEnum().WithMessage("Status is invalid.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be 0 or greater.")
            .LessThanOrEqualTo(10000).WithMessage("Sort order must be at most 10000.");

        AdminValidationShared.TagList(RuleFor(x => x.Technologies), 20, 60);

        RuleFor(x => x.CompletedOn)
            .GreaterThanOrEqualTo(x => x.StartedOn)
            .When(x => x.StartedOn.HasValue && x.CompletedOn.HasValue)
            .WithMessage("Completed date must be on or after the start date.");
    }
}

/// <summary>Rules for admin blog create/update.</summary>
public sealed class UpsertBlogPostRequestValidator : AbstractValidator<UpsertBlogPostRequest>
{
    public UpsertBlogPostRequestValidator()
    {
        AdminValidationShared.Slug(RuleFor(x => x.Slug));

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title must be at most 300 characters.");

        RuleFor(x => x.Excerpt)
            .NotEmpty().WithMessage("Excerpt is required.")
            .MaximumLength(1000).WithMessage("Excerpt must be at most 1000 characters.");

        RuleFor(x => x.BodyMarkdown)
            .NotEmpty().WithMessage("Body is required.")
            .MaximumLength(100000).WithMessage("Body must be at most 100000 characters.");

        AdminValidationShared.OptionalUrl(RuleFor(x => x.CoverImageUrl));

        RuleFor(x => x.ReadingMinutes)
            .GreaterThanOrEqualTo(0).WithMessage("Reading minutes must be 0 or greater.")
            .LessThanOrEqualTo(600).WithMessage("Reading minutes must be at most 600.");

        AdminValidationShared.TagList(RuleFor(x => x.Tags), 20, 60);
    }
}
