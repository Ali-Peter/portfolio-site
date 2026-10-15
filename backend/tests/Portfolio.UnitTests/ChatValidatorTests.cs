using FluentValidation;
using Portfolio.Application.Common;
using Portfolio.Application.Dto;
using Portfolio.Application.Validators;

namespace Portfolio.UnitTests;

/// <summary>
/// Input limits for the public assistant endpoint — every rejection surfaces
/// as 422 ProblemDetails downstream.
/// </summary>
public sealed class ChatValidatorTests
{
    private readonly IValidator<CreateChatRequest> _validator = new CreateChatRequestValidator();

    [Fact]
    public async Task ReasonableQuestion_Passes()
    {
        var result = await _validator.ValidateAsync(new CreateChatRequest("What projects use ASP.NET?"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyQuestion_Fails(string message)
    {
        var result = await _validator.ValidateAsync(new CreateChatRequest(message));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateChatRequest.Message));
    }

    [Fact]
    public async Task Question_OverMaxLength_Fails()
    {
        var result = await _validator.ValidateAsync(
            new CreateChatRequest(new string('a', CreateChatRequestValidator.MaxMessageLength + 1)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateChatRequest.Message));
    }

    [Fact]
    public async Task Question_AtMaxLength_Passes()
    {
        var result = await _validator.ValidateAsync(
            new CreateChatRequest(new string('a', CreateChatRequestValidator.MaxMessageLength)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void DefaultOptions_StayWithinSafePublicLimits()
    {
        var options = new AiAssistantOptions();

        Assert.InRange(options.TopK, 1, 20);
        Assert.Equal(2000, options.MaxQuestionChars);
        Assert.True(options.MaxContextChars > 0);
    }
}