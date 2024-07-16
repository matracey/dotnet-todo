using System.Net.Http.Json;
using TodoBlazorApp.Models;

namespace TodoBlazorApp.Services;

public class TodoService
{
    private readonly HttpClient _httpClient;

    public TodoService(HttpClient httpClient)
    {
          _httpClient = httpClient;
          _httpClient.BaseAddress = new Uri("https://localhost:5000/"); // Update the port number if necessary
    }

    public async Task<IEnumerable<TodoItem>> GetTodosAsync()
    {
        return await _httpClient.GetFromJsonAsync<IEnumerable<TodoItem>>("api/todo") ?? [];
    }

    public async Task<TodoItem?> GetTodoAsync(long id)
    {
        return await _httpClient.GetFromJsonAsync<TodoItem>($"api/todo/{id}");
    }

    public async Task AddTodoAsync(TodoItem todo)
    {
        await _httpClient.PostAsJsonAsync("api/todo", todo);
    }

    public async Task UpdateTodoAsync(TodoItem todo)
    {
        await _httpClient.PutAsJsonAsync($"api/todo/{todo.Id}", todo);
    }

    public async Task DeleteTodoAsync(long id)
    {
        await _httpClient.DeleteAsync($"api/todo/{id}");
    }
}