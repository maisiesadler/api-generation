using AutoFixture;
using Example.Interactors;
using Example.Models;

namespace Example.GeneratedImplementations
{
    public class GetApiTodoIdInteractor : IGetApiTodoIdInteractor
    {
        private readonly Fixture _fixture = new Fixture();
        public async Task<ToDoItem> Execute(string xRequestId, int id, string type)
        {
            return _fixture.Create<ToDoItem>();
        }
    }
}