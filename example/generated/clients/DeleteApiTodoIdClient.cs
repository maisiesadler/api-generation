using Example.Models;

namespace Example.Clients
{
    public class DeleteApiTodoIdClient
    {
        public DeleteApiTodoIdClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;
        public async Task<ClientResponse> Execute()
        {
        }
    }
}