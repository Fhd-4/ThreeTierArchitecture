namespace Project.DAL.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssignTo { get; set; } = string.Empty;
}