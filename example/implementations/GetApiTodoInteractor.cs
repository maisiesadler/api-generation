using Example.Models;
using Example.Interactors;

namespace Example.Implementations
{
    public class GetApiTodoInteractor : IGetApiTodoInteractor
    {
        public Task<ToDoItem[]> Execute() => Task.FromResult(
            new[] { new ToDoItem { Id = 123, Name = "pizza", IsCompleted = false } }
        );
    }
}
