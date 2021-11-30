using System.Text.Json.Serialization;

namespace Example.Models
{
    public record ToDoItem
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; init; }
    }
}