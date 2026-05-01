namespace EShop.Ordering.Domain.Abstractions;

public abstract class Entity<T> : IEntity<T>
{
    public T Id { get; set; } = default!;

    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}

public interface IEntity<T> : IEntity
{
    T Id { get; set; }
}

public interface IEntity
{
    DateTime? CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
