using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using System.Text.Json.Serialization;

namespace LlamaWebApi.Services
{
    public class LlamaChatCompletionService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly IReadOnlyDictionary<string, object?> _attributes;

        public LlamaChatCompletionService(HttpClient httpClient, string baseUrl = "http://localhost:8080/", string apiToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6IjQ3MTU0ZWM5LTU3Y2QtNDNmYy05M2RkLTBjNjgyNDE1MTcxMiJ9.whPLdvYlK3ygPW7G0zByUOBmjjovhLthWzgoJa1v8wM")
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);
            _attributes = new Dictionary<string, object?> { { "ModelName", "LLaMA" } }.AsReadOnly();
        }

        public IReadOnlyDictionary<string, object?> Attributes => _attributes;

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            var request = new
            {
                model = "llama3.2:latest",
                messages = chatHistory.Select(m => new { role = m.Role.ToString().ToLower(), content = m.Content }).ToArray()
            };
            Console.WriteLine($"Sending POST request: {System.Text.Json.JsonSerializer.Serialize(request)}");

            var response = await _httpClient.PostAsJsonAsync("api/chat/completions", request, cancellationToken);
            string rawResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}, Raw response: {rawResponse}");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed with status: {response.StatusCode}, response: {rawResponse}");

            var result = System.Text.Json.JsonSerializer.Deserialize<ChatCompletionResponse>(rawResponse);
            return result.Choices.Select(c => new ChatMessageContent(AuthorRole.Assistant, c.Message.Content)).ToList();
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class ChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public Choice[] Choices { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("logprobs")]
        public object Logprobs { get; set; }  // Null in your example, so object allows flexibility

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
