[Read this on GitHub](https://github.com/matracey/dotnet-todo/blob/main/README.md)

## Step 1: Set Up Your Development Environment

1. **Install .NET SDK:** Ensure you have the .NET SDK installed. You can download it from the [.NET download page](https://dotnet.microsoft.com/download). You might need to restart your terminal or command prompt after installing the SDK to ensure the `dotnet` command is available.

   - You can verify that the .NET SDK is installed by running the following command in your terminal or command prompt:

     ```bash
     dotnet --version
     ```

   - If the command returns a version number, the .NET SDK is installed correctly.

2. **Install C# Dev Kit for VS Code**: Install the [C# Dev Kit extension for VS Code]([C# Dev Kit - Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)), which makes working with a .NET solution much easier.

3. **Open command prompt**: We'll be using some simple command line steps to create our solution. You can use Windows Terminal on Windows (or Command Prompt if you don't have Windows Terminal), or iTerm / Terminal on macOS.

## Step 2: Create the Solution and Projects

[View source code on GitHub](https://github.com/matracey/dotnet-todo/tree/end-step-2)
[Download zip file from this step](https://github.com/matracey/dotnet-todo/archive/refs/tags/end-step-2.zip)

1. **Create a New Solution:**

   ```bash
   mkdir DotnetTodo
   cd DotnetTodo
   dotnet new sln
   ```

2. **Create the ASP.NET Web API Project:**

   ```bash
   dotnet new webapi --use-controllers -o src/TodoApi
   dotnet sln add src/TodoApi/TodoApi.csproj
   ```

3. **Create the Blazor WebAssembly Project:**

   ```bash
   dotnet new blazorwasm -o src/TodoBlazorApp
   dotnet sln add src/TodoBlazorApp/TodoBlazorApp.csproj
   ```

4. **Open our workspace in VS Code:**

```shell
code . # This will open the current folder in VS Code.
```

## Step 3: Set Up the ASP.NET Web API

[View source code on GitHub](https://github.com/matracey/dotnet-todo/tree/end-step-3)
[Download zip file from this step](https://github.com/matracey/dotnet-todo/archive/refs/tags/end-step-3.zip)

1. **Define the Todo Model:**
   - Create a new file `TodoItem.cs` inside the `src/TodoApi/Models` directory:

     ```csharp
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
     ```

2. **Install Entity Framework**:
   - Run this command from the command line to add the Entity Framework InMemory package to the project:

       ```shell
       dotnet add src/TodoApi/TodoApi.csproj package Microsoft.EntityFrameworkCore.InMemory
       ```

3. **Create the Todo Context:**
   - Create a new file `TodoContext.cs` inside the `src/TodoApi/Data` directory:

     ```csharp
      using Microsoft.EntityFrameworkCore;  
      using TodoApi.Models;  

      namespace TodoApi.Data;  

      public class TodoContext : DbContext  
      {  
          public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }  

          public DbSet<TodoItem> TodoItems { get; set; }  
      }
     ```

      - This `TodoContext` class will allow us to fetch a `TodoItem` from our "database".

4. **Register the `TodoContext` as a *Service* in `Program.cs`:**
   - Open `Program.cs` and update its content to the following:

    ```csharp
      using Microsoft.EntityFrameworkCore;
      using TodoApi.Data;

      var builder = WebApplication.CreateBuilder(args);

      // Add services to the container.

      builder.Services.AddControllers();
      // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
      builder.Services.AddEndpointsApiExplorer();
      builder.Services.AddSwaggerGen();
      builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));

      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if (app.Environment.IsDevelopment())
      {
          app.UseSwagger();
          app.UseSwaggerUI();
      }

      app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(_ => true).AllowCredentials());
      app.UseHttpsRedirection();

      app.UseRouting();

      app.MapControllers();

      app.Run();
    ```

5. **Create the Todo Controller:**
   - Create a new file `TodoController.cs` inside the `src/TodoApi/Controllers` directory:

     ```csharp  
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
     ```

6. Add `GetTodoItem` to get an individual `TodoItem` after the `GetTodoItems` method (line 26).

     ```csharp  
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
     ```

7. Add `PostTodoItem` to create a `TodoItem` after the `GetTodoItem` method (line 39).

     ```csharp  
    [HttpPost]
    public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
    {
        _todoContext.TodoItems.Add(todoItem);
        await _todoContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
    }
     ```

8. Add `PutTodoItem` to update a `TodoItem` after the `PostTodoItem` method (line 48).

     ```csharp  
    [HttpPut("{id:long}")]
    public async Task<IActionResult> PutTodoItem(long id, TodoItem todoItem)
    {
        if (id != todoItem.Id)
        {
            return BadRequest();
        }

        // Use Entity Framework to find the database entry matching todoItem and mark it as "Modified"
        _todoContext.Entry(todoItem).State = EntityState.Modified;

        // Saving will commit the modified todoItem to our database.
        await _todoContext.SaveChangesAsync();

        return NoContent();
    }
     ```

9. Finally, Add `DeleteTodoItem` to delete a `TodoItem` after the `PutTodoItem` method (line 65).

     ```csharp  
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteTodoItem(long id)
    {
        var todoItem = await _todoContext.TodoItems.FindAsync(id);
        if (todoItem == null)
        {
            return NotFound();
        }

        _todoContext.TodoItems.Remove(todoItem);
        await _todoContext.SaveChangesAsync();

        return NoContent();
    }
     ```

10. **Run the API Project:**

      ```bash
      cd src
      cd TodoApi
      dotnet run
      ```

    - The output of the `dotnet run` command should print the port that the server is running under, such as `http://localhost:5000` or `http://localhost:5001`

11. Open the Swagger UI:

    - Navigate to `/swagger` under the address of your .NET server. For example, `http://localhost:5000/swagger` or `http://localhost:5001/swagger`
    - You should see all of the API methods we added earlier listed here.
    - Take note of the port number being used by our API server. We'll need this later.

## Step 4: Set Up the Blazor WebAssembly Application

[View source code on GitHub](https://github.com/matracey/dotnet-todo/tree/end-step-4)
[Download zip file from this step](https://github.com/matracey/dotnet-todo/archive/refs/tags/end-step-4.zip)

1. **Create Todo View Model:**
   - Create a new file `TodoItem.cs` inside the `src/TodoBlazorApp/Models` directory:

     ```csharp
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
     ```

2. **Create TodoService:**
   - Create a new file `TodoService.cs` inside the `src/TodoBlazorApp/Services` directory:

     ```csharp
      using System.Net.Http.Json;
      using TodoBlazorApp.Models;

      namespace TodoBlazorApp.Services;

      public class TodoService
      {
          private readonly HttpClient _httpClient;

          public TodoService(HttpClient httpClient)
          {
              _httpClient = httpClient;
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
     ```

     - Each of the methods in **`TodoService`** directly correlates to the methods we added to our **`TodoController`** in the **`TodoApi`** project.

3. **Add `BaseAddress` configuration to `HttpClient`**:
    - Using the port number of our API service that you noted earlier, update the `TodoService` constructor to set the `BaseAddress` of the `HttpClient`:

     ```csharp
      public TodoService(HttpClient httpClient)
      {
          _httpClient = httpClient;
          _httpClient.BaseAddress = new Uri("https://localhost:5001/"); // Update the port number if necessary
      }
      ```

4. **Register `TodoService` in `Program.cs`:**
   - Modify `src/TodoBlazorApp/Program.cs` to register the service:

     ```csharp
     builder.Services.AddScoped<TodoService>();

     // Existing code...
     await builder.Build().RunAsync();
     ```

5. **Update `_Imports.razor`**:
   - Add the following lines to `src/TodoBlazorApp/_Imports.razor`:

    ```blazor
    @using TodoBlazorApp.Models  
    @using TodoBlazorApp.Services
    ```

6. **Update Home Component:**
   - Open `src/TodoBlazorApp/Pages/Home.razor` and replace its content with:

     ```razor
      @page "/"
      @inject TodoService TodoService

      <h3>Todo List</h3>

      <div class="container mt-5">
          <h1 class="text-center mb-4">To Do List</h1>
          <div class="row justify-content-center">
              <div class="col-md-8">
                  <div class="card">
                      <div class="card-body">
                          <div class="input-group mb-3">
                              <form @onsubmit="AddTodo" class="input-group mb-3">
                                  <input type="text" class="form-control" id="todo-input" placeholder="Add new task..." @bind="_newTodoText" />
                                  <button class="btn btn-primary" type="submit">Add</button>
                              </form>
                          </div>
                          <ul class="list-group" id="todo-list">
                              @foreach (var todo in _todos)
                              {
                              <li class="list-group-item d-flex justify-content-between align-items-center">
                                  <input type="checkbox" @onclick="() => ToggleTodo(todo)" checked="@(todo.IsComplete ? "checked" : null)" />
                                  <span class="task-text">@todo.Text</span>
                                  <div class="btn-group">
                                      <button class="btn btn-danger btn-sm delete-btn" @onclick="() => DeleteTodo(todo.Id)">✕</button>
                                  </div>
                              </li>
                              }
                          </ul>
                      </div>
                  </div>
              </div>
          </div>
      </div>

      @code {
          private IEnumerable<TodoItem> _todos = [];
          private string _newTodoName = "";

          protected override async Task OnInitializedAsync()
          {
              _todos = await TodoService.GetTodosAsync();
          }

          private async Task AddTodo()
          {
              throw new NotImplementedException();
          }

          private async Task ToggleTodo(TodoItem todo)
          {
              throw new NotImplementedException();
          }

          private async Task DeleteTodo(long id)
          {
              throw new NotImplementedException();
          }
      }
     ```

7. **Implement Home page methods:**
    - Implement the `AddTodo`, `ToggleTodo`, and `DeleteTodo` methods in the `src/TodoBlazorApp/Pages/Home.razor` file:

   ```csharp
    private async Task AddTodo()
    {
        if (!string.IsNullOrEmpty(_newTodoText))
        {
            var newTodo = new TodoItem (_newTodoText);
            await TodoService.AddTodoAsync(newTodo);
            _todos = await TodoService.GetTodosAsync();
            _newTodoText = string.Empty;
        }
    }

    private async Task ToggleTodo(TodoItem todo)
    {
        todo.IsComplete = !todo.IsComplete;
        await TodoService.UpdateTodoAsync(todo);
    }

    private async Task DeleteTodo(long id)
    {
        await TodoService.DeleteTodoAsync(id);
        _todos = await TodoService.GetTodosAsync();
    }
   ```

## Step 5: Run the Solution

[View source code on GitHub](https://github.com/matracey/dotnet-todo/tree/end-step-5)
[Download zip file from this step](https://github.com/matracey/dotnet-todo/archive/refs/tags/end-step-5.zip)

1. **Run the API Project (if it isn't already running):**

   ```bash
   cd src
   cd TodoApi
   dotnet run
   ```

2. **Run the Blazor Project:**
   Open a new terminal and run:

   ```bash
   cd src
   cd TodoBlazorApp
   dotnet run
   ```

Now, you should have a basic Todo solution with an ASP.NET Web API backend and a Blazor WebAssembly frontend.

You can open your browser and navigate to the Blazor application, which may be running on `http://localhost:5001` depending on the contents of `src/TodoBlazorApp/Properties/launchSettings.json`.

The API should be available at `http://localhost:5000/api/todo`, depending on the contents of `src/TodoApi/Properties/launchSettings.json`.
