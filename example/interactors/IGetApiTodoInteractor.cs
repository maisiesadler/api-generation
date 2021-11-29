namespace CodeGen.Interactors
{
    public interface IGetApiTodoInteractor
    {
        Task<ToDoItem> Execute();
    }
}