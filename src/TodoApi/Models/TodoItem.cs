namespace TodoApi.Models;  

public record TodoItem
{
    public TodoItem(string name, bool isComplete = false)
    {
        Name = name;
        IsIsComplete = isComplete;
    }

    public long Id { get; set; }
    public string Name { get; set; }
    public bool IsIsComplete { get; set; }
}