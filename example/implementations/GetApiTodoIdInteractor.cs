using Example.Interactors;
using Example.Models;

namespace Example.Implementations
{
    public class GetApiTodoIdInteractor : IGetApiTodoIdInteractor
    {
        public Task<ToDoItem> Execute(int id) => Task.FromResult(new ToDoItem { Id = id, Name = "pizza", IsCompleted = true });
    }
}
