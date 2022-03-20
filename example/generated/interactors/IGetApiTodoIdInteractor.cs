using Example.Models;

namespace Example.Interactors
{
    public interface IGetApiTodoIdInteractor
    {
        Task<ToDoItem> Execute(string xRequestId, int id, string type);
    }
}