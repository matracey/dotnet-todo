namespace TodoBlazorApp.Models;

public record TodoItem
{
    public TodoItem(string name, bool isComplete = false)
    {
        Name = name;
        IsComplete = isComplete;
    }

    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsComplete { get; set; }
}