namespace TodoBlazorApp.Models;

public record TodoItem
{
    public TodoItem(string text, bool isComplete = false)
    {
        Text = text;
        IsComplete = isComplete;
    }

    public long Id { get; set; }
    public string Text { get; set; }
    public bool IsComplete { get; set; }
}