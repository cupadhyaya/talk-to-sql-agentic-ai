using System.Text;
using AgenticAIDemo.Models;
using Newtonsoft.Json;

namespace AgenticAIDemo.Services;

public sealed class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiService(string apiKey, string model)
    {
        _apiKey = apiKey;
        _model = model;
        _httpClient = new HttpClient();
    }

    public bool HasValidApiKey()
    {
        return !string.IsNullOrWhiteSpace(_apiKey)
               && _apiKey != "YOUR_GEMINI_API_KEY_HERE";
    }

    public async Task<GeminiDecision> GetNextStepAsync(
        string userQuestion,
        string schemaDescription,
        List<AgentStep> previousSteps)
    {
        var history = previousSteps.Count == 0
            ? "No previous steps yet."
            : string.Join("\n", previousSteps.Select(s =>
                $"Step {s.StepNumber}: Thought={s.Thought}, Action={s.Action}, Input={s.ActionInput}, Result={s.Observation}"));

        var systemPrompt = """
            You are an Agentic AI assistant for a small shop database.
            You help users by thinking step-by-step and choosing ONE action.

            Database schema:
            """ + schemaDescription + """

            Available actions (reply with JSON only, no markdown):
            1. "run_sql" - Run a read-only SELECT query. Put the SQL in actionInput.
            2. "answer_user" - Give the final friendly answer. Put the answer in actionInput.

            Rules:
            - Use run_sql when you need real data from Customers or Orders tables.
            - Only write SELECT queries. Never INSERT, UPDATE, DELETE.
            - After you have enough data, use answer_user.
            - Keep answers simple and in plain English.
            - For counts and totals, use SQL rather than guessing.

            Reply format (strict JSON):
            {"thought":"your reasoning","action":"run_sql or answer_user","actionInput":"SQL query or final answer"}
            """;

        var userPrompt = $"""
            User question: {userQuestion}

            Previous steps:
            {history}

            What is the next step?
            """;

        var requestBody = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = systemPrompt } }
            },
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = userPrompt } }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json"
            }
        };

        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var json = JsonConvert.SerializeObject(requestBody);

        HttpContent content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(url, content);
        var result = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini API error ({response.StatusCode}): {result}");
        }

        var text = ExtractTextFromGeminiResponse(result);
        return ParseDecision(text);
    }

    private static string ExtractTextFromGeminiResponse(string json)
    {
        dynamic? data = JsonConvert.DeserializeObject(json);
        if (data is null)
        {
            return json;
        }

        try
        {
            return data.candidates[0]
                       .content
                       .parts[0]
                       .text
                       .ToString();
        }
        catch
        {
            return json;
        }
    }

    private static GeminiDecision ParseDecision(string rawText)
    {
        var cleaned = rawText.Trim();
        if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned
                .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "")
                .Trim();
        }

        dynamic? data = JsonConvert.DeserializeObject(cleaned);
        if (data is null)
        {
            return new GeminiDecision
            {
                Thought = "Could not parse model response.",
                Action = "answer_user",
                ActionInput = rawText
            };
        }

        try
        {
            return new GeminiDecision
            {
                Thought = data.thought?.ToString() ?? "",
                Action = data.action?.ToString() ?? "answer_user",
                ActionInput = data.actionInput?.ToString() ?? ""
            };
        }
        catch
        {
            return new GeminiDecision
            {
                Thought = "Could not parse model response.",
                Action = "answer_user",
                ActionInput = rawText
            };
        }
    }
}
