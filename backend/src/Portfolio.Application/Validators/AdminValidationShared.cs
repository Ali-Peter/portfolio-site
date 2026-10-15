using FluentValidation;

namespace Portfolio.Application.Validators;

/// <summary>Shared rules for admin project/blog/skill/experience payloads.</summary>
internal static class AdminValidationShared
{
    internal static IRuleBuilderOptions<T, string> Slug<T>(IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Slug is required.")
            .MinimumLength(2).WithMessage("Slug must be at least 2 characters.")
            .MaximumLength(200).WithMessage("Slug must be at most 200 characters.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug must be lowercase letters, digits and hyphens.");

    internal static IRuleBuilderOptions<T, string?> OptionalUrl<T>(IRuleBuilder<T, string?> rule) =>
        rule.MaximumLength(1000).WithMessage("URL must be at most 1000 characters.")
            .Must(url => string.IsNullOrWhiteSpace(url) || Uri.TryCreate(url.Trim(), UriKind.Absolute, out _))
            .WithMessage("URL must be a valid absolute URL.");

    internal static IRuleBuilderOptions<T, IReadOnlyList<string>?> TagList<T>(
        IRuleBuilder<T, IReadOnlyList<string>?> rule, int maxItems, int maxLength) =>
        rule.Must(tags => tags == null || tags.Count <= maxItems)
                .WithMessage($"At most {maxItems} entries are allowed.")
            .Must(tags => tags == null || tags.All(t => !string.IsNullOrWhiteSpace(t) && t.Trim().Length <= maxLength))
                .WithMessage($"Each entry must be 1–{maxLength} characters.");
}
