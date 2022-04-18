using Example.Models;

namespace Example.Clients
{
    public class GetApiTodoIdClient
    {
        public GetApiTodoIdClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;
        public async Task<ToDoItem> Execute(string xRequestId, int id, string type)
        {
        }
    }
}