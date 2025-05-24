using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Structure.Command
{
    public static class ChatGPTHandler
    {
        public static async Task<string> CallChatGPT(string prompt)
        {
            // 🔐 Your actual DeepSeek API key
            string apiKey = "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";

            if (string.IsNullOrEmpty(apiKey))
                return "API key is missing. Please insert your DeepSeek API key.";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                model = "deepseek/deepseek-r1:free",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // 🌐 DeepSeek endpoint (OpenAI-compatible if they follow the same format)
            var response = await client.PostAsync("https://api.deepseek.com/v1/chat/completions", content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"HTTP Error {response.StatusCode}:\n{result}";

            using var doc = JsonDocument.Parse(result);

            if (doc.RootElement.TryGetProperty("error", out var error))
            {
                var message = error.GetProperty("message").GetString();
                return $"DeepSeek Error: {message}";
            }

            try
            {
                var reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return reply;
            }
            catch (Exception ex)
            {
                return $"Unexpected response format: {ex.Message}\nRaw response:\n{result}";
            }
        }
    }
}
