using Example.Interactors;

namespace Example.Implementations
{
    public class GetApiTodoIdInteractor : IGetApiTodoIdInteractor
    {
        public Task Execute() => Task.CompletedTask;
    }
}