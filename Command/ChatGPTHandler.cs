using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Structure.Command
{
    public static class ChatGPTHandler
    {
        public static async Task<string> CallGeminiAsync(string prompt)
        {
            string apiKey = "AIzaSyDCVxCNVPdrfILciqewHpYU-u9kfyDRjcs";
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(endpoint, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"HTTP Error {response.StatusCode}:\n{result}";

            try
            {
                var doc = JsonDocument.Parse(result);
                return doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch (Exception ex)
            {
                return $"Unexpected format: {ex.Message}\n\nRaw response:\n{result}";
            }
        }
    }
}