using Example.Interactors;
using Example.Models;

namespace Example.Implementations
{
    public class GetApiTodoIdInteractor : IGetApiTodoIdInteractor
    {
        public Task<ToDoItem> Execute(int id, string type) => Task.FromResult(new ToDoItem { Id = id, Name = "pizza is " + type, IsCompleted = true });
    }
}
