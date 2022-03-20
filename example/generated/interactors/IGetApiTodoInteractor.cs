using Example.Models;

namespace Example.Interactors
{
    public interface IGetApiTodoInteractor
    {
        Task<ToDoItem[]> Execute();
    }
}