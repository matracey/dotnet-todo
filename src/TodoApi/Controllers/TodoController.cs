using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    private readonly TodoContext _todoContext;

    public TodoController(TodoContext todoContext)
    {
        _todoContext = todoContext;

        CreateSampleTodo("Item1", false);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
    {
        return await _todoContext.TodoItems.ToListAsync();
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
    {
        var todoItem = await _todoContext.TodoItems.FindAsync(id);

        if (todoItem == null)
        {
            return NotFound();
        }

        return todoItem;
    }

    private void CreateSampleTodo(string todoText, bool todoIsComplete)
    {
        if (_todoContext.TodoItems.Any())
        {
            return;
        }

        // Create a sample TodoItem if one doesn't already exist...
        _todoContext.TodoItems.Add(new TodoItem(todoText, todoIsComplete));
        _todoContext.SaveChanges();
    }
}