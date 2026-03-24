using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class WordOfWonderAiClient
{
    private const string DEFAULT_API_URL = "https://api.openai.com/v1/chat/completions";
    private static readonly HttpClient HttpClient = new HttpClient();

    public static async Task<List<string>> GenerateExtraWordsAsync(
        string apiKey,
        string model,
        string letters,
        int maxWords)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("AI API key dang rong.");
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("AI model dang rong.");
        }

        if (string.IsNullOrWhiteSpace(letters))
        {
            throw new ArgumentException("Letters dang rong.");
        }

        if (maxWords <= 0)
        {
            maxWords = 8;
        }

        object requestBody = new
        {
            model = model,
            temperature = 0.2f,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You generate valid single dictionary words for a word puzzle. Return strict JSON."
                },
                new
                {
                    role = "user",
                    content =
                        $"Letters: {letters}\n" +
                        $"Generate up to {maxWords} meaningful single words that can be formed from these letters. " +
                        "A letter cannot be used more times than available. " +
                        "Return JSON only with shape: {\"extra\":[\"WORD1\",\"WORD2\"]}. " +
                        "Uppercase, no spaces, no punctuation."
                }
            }
        };

        string payload = JsonConvert.SerializeObject(requestBody);

        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, DEFAULT_API_URL))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await HttpClient.SendAsync(request);
            string rawResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"AI API loi ({(int)response.StatusCode}): {rawResponse}");
            }

            return ExtractWords(rawResponse, maxWords);
        }
    }

    private static List<string> ExtractWords(string chatCompletionsResponse, int maxWords)
    {
        JObject root = JObject.Parse(chatCompletionsResponse);
        string content = root["choices"]?[0]?["message"]?["content"]?.ToString();

        if (string.IsNullOrWhiteSpace(content))
        {
            return new List<string>();
        }

        List<string> parsed = new List<string>();
        try
        {
            JObject contentJson = JObject.Parse(content);
            JToken extraToken = contentJson["extra"] ?? contentJson["words"];
            if (extraToken is JArray extraArray)
            {
                foreach (JToken token in extraArray)
                {
                    string value = token?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        parsed.Add(value.Trim().ToUpperInvariant());
                    }
                }
            }
        }
        catch
        {
            string[] fallbackTokens = content
                .Replace(",", "\n")
                .Replace(";", "\n")
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in fallbackTokens)
            {
                parsed.Add(token.Trim().ToUpperInvariant());
            }
        }

        return parsed
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .Distinct(StringComparer.Ordinal)
            .Take(maxWords)
            .ToList();
    }
}
