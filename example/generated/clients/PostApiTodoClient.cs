using Example.Models;

namespace Example.Clients
{
    public class PostApiTodoClient
    {
        public PostApiTodoClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;
        public async Task<ClientResponse> Execute()
        {
            var request = new HttpRequestMessage{Method = HttpMethod.Post, };
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return new ClientResponse(response.StatusCode, content);
        }
    }
}