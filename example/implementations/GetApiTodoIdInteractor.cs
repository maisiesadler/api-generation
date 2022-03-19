using Example.Interactors;
using Example.Models;

namespace Example.Implementations
{
    public class GetApiTodoIdInteractor : IGetApiTodoIdInteractor
    {
        public Task<ToDoItem> Execute() => Task.FromResult(new ToDoItem { Id = 1, Name = "pizza", IsCompleted = true });
    }
}
