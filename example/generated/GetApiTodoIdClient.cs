using Example.Models;

namespace Example.Clients
{
    public class GetApiTodoIdClient
    {
        private readonly HttpClient _httpClient;
        public async Task<ToDoItem> Execute(string xRequestId, int id, string type)
        {
        }
    }
}