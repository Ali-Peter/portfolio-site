using AutoMapper;
using FluentValidation;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Portfolio.Application.Dto;
using Portfolio.Application.Mapping;
using Portfolio.Application.Services;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

/// <summary>Contact intake against real PostgreSQL: persistence, honeypot, validation.</summary>
[Collection(PostgresCollection.Name)]
public sealed class ContactServiceTests
{
    private static readonly IConfigurationProvider Mapping = new MapperConfiguration(
        cfg => cfg.AddProfile<PortfolioMappingProfile>(),
        NullLoggerFactory.Instance);

    private readonly PostgresFixture _fixture;

    public ContactServiceTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private PortfolioDbContext CreateContext() => _fixture.CreateContext();

    private static ContactService CreateService(PortfolioDbContext db) =>
        new(db, Mapping.CreateMapper(), new Application.Validators.CreateContactMessageRequestValidator());

    [Fact]
    public async Task ValidSubmission_PersistsWithHashedIp()
    {
        await using var db = CreateContext();
        var service = CreateService(db);

        var accepted = await service.SubmitAsync(
            new CreateContactMessageRequest(
                "Adaeze Obi", "adaeze@example.com", "Hello — I liked your RAG work. Can we talk?", null),
            "203.0.113.7",
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, accepted.Id);

        var stored = await db.ContactMessages.SingleAsync(
            m => m.Id == accepted.Id, CancellationToken.None);
        Assert.Equal("Adaeze Obi", stored.Name);
        Assert.False(stored.IsHandled);
        Assert.NotNull(stored.IpHash);
        Assert.DoesNotContain("203.0.113.7", stored.IpHash, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HoneypotFilled_AcceptedWithoutPersisting()
    {
        await using var db = CreateContext();
        var before = await db.ContactMessages.CountAsync(CancellationToken.None);
        var service = CreateService(db);

        var accepted = await service.SubmitAsync(
            new CreateContactMessageRequest("Bot", "bot@spam.example", "Buy now, cheap pills.", "http://x.example"),
            "198.51.100.9",
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, accepted.Id);
        var after = await db.ContactMessages.CountAsync(CancellationToken.None);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task InvalidSubmission_ThrowsValidation()
    {
        await using var db = CreateContext();
        var service = CreateService(db);

        await Assert.ThrowsAsync<ValidationException>(
            () => service.SubmitAsync(
                new CreateContactMessageRequest("A", "not-an-email", "short", null),
                null,
                CancellationToken.None));
    }
}
