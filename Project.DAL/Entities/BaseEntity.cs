namespace Project.DAL.Entities;

public abstract class BaseEntity
{
    public string Id { get; protected set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}