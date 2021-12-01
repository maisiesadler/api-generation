using Example.Interactors;

namespace Example.Implementations
{
    public class PostApiTodoInteractor : IPostApiTodoInteractor
    {
        public Task Execute() => Task.CompletedTask;
    }
}