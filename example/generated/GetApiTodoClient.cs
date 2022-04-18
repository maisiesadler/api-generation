using Example.Models;

namespace Example.Clients
{
    public class GetApiTodoClient
    {
        public GetApiTodoClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;
        public async Task<ToDoItem[]> Execute()
        {
        }
    }
}