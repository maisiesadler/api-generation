namespace CodeGen.Models
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