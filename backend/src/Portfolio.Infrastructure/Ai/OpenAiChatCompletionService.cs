using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Exceptions;
using Portfolio.Application.Services;

namespace Portfolio.Infrastructure.Ai;

/// <summary>
/// <c>POST /v1/chat/completions</c> client with <c>stream: true</c>.
/// Parses the provider's SSE frames and yields only answer text deltas;
/// token usage (when reported) is logged for basic observability.
/// </summary>
public sealed class OpenAiChatCompletionService : IChatCompletionService
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiChatCompletionService> _logger;

    public OpenAiChatCompletionService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiChatCompletionService> logger)
    {
        _http = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        string systemPrompt,
        string userPrompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            throw new AiUnavailableException("The chat provider is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(new
            {
                model = _options.ChatModel,
                stream = true,
                stream_options = new { include_usage = true },
                temperature = 0.2,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt },
                },
            }),
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _http.SendAsync(
            request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "OpenAI chat request failed ({StatusCode}): {Body}",
                (int)response.StatusCode,
                Truncate(errorBody));
            throw new InvalidOperationException(
                $"Chat provider returned {(int)response.StatusCode}.");
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue; // event:/comment/keep-alive lines carry nothing we need
            }

            var payload = line["data:".Length..].Trim();
            if (payload == "[DONE]")
            {
                yield break;
            }

            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (root.TryGetProperty("usage", out var usage))
            {
                // Logged once per request — the final stream chunk carries usage.
                _logger.LogInformation(
                    "OpenAI usage: {PromptTokens} prompt + {CompletionTokens} completion tokens ({Model}).",
                    usage.TryGetProperty("prompt_tokens", out var pt) ? pt.GetInt32() : 0,
                    usage.TryGetProperty("completion_tokens", out var ct) ? ct.GetInt32() : 0,
                    _options.ChatModel);
            }

            if (root.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("delta", out var delta) &&
                delta.TryGetProperty("content", out var content) &&
                content.ValueKind == JsonValueKind.String)
            {
                var text = content.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
    }

    private static string Truncate(string value) =>
        value.Length <= 400 ? value : value[..400] + "…";
}