using AutoFixture;
using Example.Interactors;
using Example.Models;

namespace Example.GeneratedImplementations
{
    public class GetApiTodoInteractor : IGetApiTodoInteractor
    {
        private readonly Fixture _fixture = new Fixture();
        public async Task<ToDoItem[]> Execute()
        {
            return _fixture.Create<ToDoItem[]>();
        }
    }
}