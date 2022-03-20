using Example.Interactors;
using Example.Models;

namespace Example.Implementations
{
    public class GetApiTodoIdInteractor : IGetApiTodoIdInteractor
    {
        public Task<ToDoItem> Execute(string xRequestId, int id, string type)
            => Task.FromResult(new ToDoItem { Id = id, Name = $"[{xRequestId}] pizza is {type}", IsCompleted = true });
    }
}
