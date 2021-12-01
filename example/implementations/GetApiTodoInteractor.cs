using Example.Models;
using Example.Interactors;

namespace Example.Implementations
{
    public class GetApiTodoInteractor : IGetApiTodoInteractor
    {
        public Task<ToDoItem> Execute() => Task.FromResult(new ToDoItem { Id = 1, Name = "pizza", IsCompleted = true });
    }
}