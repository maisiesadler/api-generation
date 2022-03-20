using Example.Models;

namespace Example.Interactors
{
    public interface IGetApiTodoIdInteractor
    {
        Task<ToDoItem> Execute(int id, string type);
    }
}