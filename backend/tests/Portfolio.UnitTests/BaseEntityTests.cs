using Portfolio.Domain.Entities;

namespace Portfolio.UnitTests;

public class BaseEntityTests
{
    [Fact]
    public void NewEntity_InitialisesIdentityAndAuditDefaults()
    {
        var entity = new TestEntity();

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.False(entity.IsDeleted);
        Assert.Null(entity.UpdatedAtUtc);

        var now = DateTime.UtcNow;
        Assert.InRange(entity.CreatedAtUtc, now.AddMinutes(-1), now.AddMinutes(1));
    }

    private sealed class TestEntity : BaseEntity;
}
