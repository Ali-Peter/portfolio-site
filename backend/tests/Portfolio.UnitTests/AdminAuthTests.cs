using FluentValidation;
using Portfolio.Application.Dto;
using Portfolio.Application.Services;
using Portfolio.Application.Validators;

namespace Portfolio.UnitTests;

/// <summary>Admin credential plumbing: PBKDF2 hashing + login payload rules.</summary>
public sealed class AdminAuthTests
{
    // ------------------------------------------------------------------ hasher

    [Fact]
    public void Hash_RoundTrips_WithVerify()
    {
        var hash = AdminPasswordHasher.Hash("correct horse battery staple");

        Assert.True(AdminPasswordHasher.Verify("correct horse battery staple", hash));
    }

    [Fact]
    public void Hash_UsesRandomSalt_EachCall()
    {
        var first = AdminPasswordHasher.Hash("same-password");
        var second = AdminPasswordHasher.Hash("same-password");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = AdminPasswordHasher.Hash("right-password");

        Assert.False(AdminPasswordHasher.Verify("wrong-password", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("v2.210000.c2FsdA==.aGFzaA==")]
    [InlineData("v1.210000.not-base64.aGFzaA==")]
    public void Verify_MalformedHash_ReturnsFalse(string stored)
    {
        Assert.False(AdminPasswordHasher.Verify("anything", stored));
    }

    // --------------------------------------------------------------- login form

    private readonly IValidator<AdminLoginRequest> _loginValidator = new AdminLoginRequestValidator();

    [Fact]
    public async Task Login_Valid_Passes()
    {
        var result = await _loginValidator.ValidateAsync(
            new AdminLoginRequest("admin@example.com", "s3cret-pass"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("not-an-email", "password")]
    [InlineData("admin@example.com", "")]
    public async Task Login_MissingOrInvalidFields_Fails(string email, string password)
    {
        var result = await _loginValidator.ValidateAsync(new AdminLoginRequest(email, password));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Login_TooLongEmail_Fails()
    {
        var email = new string('a', 310) + "@example.com";
        var result = await _loginValidator.ValidateAsync(new AdminLoginRequest(email, "password"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AdminLoginRequest.Email));
    }
}
