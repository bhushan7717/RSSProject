using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PolicyMonitor.Data.Context;
using PolicyMonitor.Data.Repositories;
using PolicyMonitor.Domain.Entities;
using Xunit;

namespace PolicyMonitor.Data.Tests.Repositories;

public class RepositoryTests
{
    private class TestEntity : BaseEntity<int>
    {
        public string Name { get; set; } = string.Empty;
    }

    private class TestDbContext : PolicyDbContext
    {
        public TestDbContext(DbContextOptions<PolicyDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }

    private PolicyDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        var entity = new TestEntity { Id = 1, Name = "Test", CreatedAt = DateTime.UtcNow };
        await context.Set<TestEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEntity_ReturnsNull()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);

        // Act
        var result = await repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_MultipleEntities_ReturnsAll()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        await context.Set<TestEntity>().AddRangeAsync(
            new TestEntity { Id = 1, Name = "Test1", CreatedAt = DateTime.UtcNow },
            new TestEntity { Id = 2, Name = "Test2", CreatedAt = DateTime.UtcNow },
            new TestEntity { Id = 3, Name = "Test3", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task FindAsync_MatchingPredicate_ReturnsMatchingEntities()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        await context.Set<TestEntity>().AddRangeAsync(
            new TestEntity { Id = 1, Name = "Active", CreatedAt = DateTime.UtcNow },
            new TestEntity { Id = 2, Name = "Inactive", CreatedAt = DateTime.UtcNow },
            new TestEntity { Id = 3, Name = "Active", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.FindAsync(e => e.Name == "Active");

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(e => e.Name.Should().Be("Active"));
    }

    [Fact]
    public async Task AddAsync_ValidEntity_AddsToDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        var entity = new TestEntity { Id = 1, Name = "New Entity", CreatedAt = DateTime.UtcNow };

        // Act
        await repository.AddAsync(entity);
        await repository.SaveChangesAsync();

        // Assert
        var saved = await context.Set<TestEntity>().FindAsync(1);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New Entity");
    }

    [Fact]
    public async Task AddAsync_NullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repository.AddAsync(null!));
    }

    [Fact]
    public async Task AddRangeAsync_ValidEntities_AddsAllToDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Entity1", CreatedAt = DateTime.UtcNow },
            new TestEntity { Id = 2, Name = "Entity2", CreatedAt = DateTime.UtcNow }
        };

        // Act
        await repository.AddRangeAsync(entities);
        await repository.SaveChangesAsync();

        // Assert
        var count = await context.Set<TestEntity>().CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEntity_UpdatesInDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        var entity = new TestEntity { Id = 1, Name = "Original", CreatedAt = DateTime.UtcNow };
        await context.Set<TestEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        entity.Name = "Updated";
        await repository.UpdateAsync(entity);
        await repository.SaveChangesAsync();

        // Assert
        var updated = await context.Set<TestEntity>().FindAsync(1);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteAsync_ExistingEntity_RemovesFromDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        var entity = new TestEntity { Id = 1, Name = "ToDelete", CreatedAt = DateTime.UtcNow };
        await context.Set<TestEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(entity);
        await repository.SaveChangesAsync();

        // Assert
        var deleted = await context.Set<TestEntity>().FindAsync(1);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ById_RemovesFromDatabase()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        var entity = new TestEntity { Id = 1, Name = "ToDelete", CreatedAt = DateTime.UtcNow };
        await context.Set<TestEntity>().AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(1);
        await repository.SaveChangesAsync();

        // Assert
        var deleted = await context.Set<TestEntity>().FindAsync(1);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task AnyAsync_MatchingPredicate_ReturnsTrue()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        await context.Set<TestEntity>().AddAsync(
            new TestEntity { Id = 1, Name = "Exists", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.AnyAsync(e => e.Name == "Exists");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_NoMatch_ReturnsFalse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);

        // Act
        var result = await repository.AnyAsync(e => e.Name == "NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_MatchingPredicate_ReturnsCorrectCount()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new Repository<TestEntity, int>(context);
        await context.Set<TestEntity>().AddRangeAsync(
            new TestEntity { Id = 1, Name = "Active", CreatedAt = DateTime.UtcNow },
            new TestEntity { Id = 2, Name = "Active", CreatedAt = DateTime.UtcNow },
            new TestEntity { Id = 3, Name = "Inactive", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await repository.CountAsync(e => e.Name == "Active");

        // Assert
        result.Should().Be(2);
    }
}
