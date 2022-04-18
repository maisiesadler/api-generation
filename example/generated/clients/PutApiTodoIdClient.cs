using Example.Models;

namespace Example.Clients
{
    public class PutApiTodoIdClient
    {
        public PutApiTodoIdClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;
        public async Task<ClientResponse> Execute()
        {
            var response = await _httpClient.SendAsync(request);
        }
    }
}