using FluentValidation;
using Portfolio.Application.Dto;
using Portfolio.Application.Validators;

namespace Portfolio.UnitTests;

/// <summary>
/// Rules for public contact submissions — every rejection surfaces as 422 downstream.
/// </summary>
public sealed class ContactValidatorTests
{
    private readonly IValidator<CreateContactMessageRequest> _validator = new CreateContactMessageRequestValidator();

    private static CreateContactMessageRequest Valid(string? website = null) =>
        new("Adaeze Obi", "adaeze@example.com", "Hello — I liked your RAG work. Can we talk?", website);

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(Valid());

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("A")]
    public async Task Name_TooShortOrMissing_Fails(string name)
    {
        var result = await _validator.ValidateAsync(Valid() with { Name = name });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateContactMessageRequest.Name));
    }

    [Fact]
    public async Task Name_TooLong_Fails()
    {
        var result = await _validator.ValidateAsync(Valid() with { Name = new string('a', 201) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateContactMessageRequest.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public async Task Email_Invalid_Fails(string email)
    {
        var result = await _validator.ValidateAsync(Valid() with { Email = email });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateContactMessageRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("too short")]
    public async Task Message_TooShortOrMissing_Fails(string message)
    {
        var result = await _validator.ValidateAsync(Valid() with { Message = message });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateContactMessageRequest.Message));
    }

    [Fact]
    public async Task Message_TooLong_Fails()
    {
        var result = await _validator.ValidateAsync(Valid() with { Message = new string('a', 5001) });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateContactMessageRequest.Message));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("http://spam.example")]
    public async Task Honeypot_NeverFailsValidation(string? website)
    {
        // The honeypot is handled by the service (silent accept), never rejected here.
        var result = await _validator.ValidateAsync(Valid(website));

        Assert.True(result.IsValid);
    }
}
