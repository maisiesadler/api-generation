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
        public async Task<ClientResponse<ToDoItem[]>> Execute()
        {
            var request = new HttpRequestMessage{Method = HttpMethod.Get, };
            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            return new ClientResponse<ToDoItem[]>(response.StatusCode, content);
        }
    }
}