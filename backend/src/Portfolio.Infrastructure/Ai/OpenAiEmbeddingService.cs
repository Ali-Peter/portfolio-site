using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Exceptions;
using Portfolio.Application.Services;

namespace Portfolio.Infrastructure.Ai;

/// <summary>
/// <c>POST /v1/embeddings</c> client for text-embedding-3-small (1536 dims).
/// Batches arrive pre-sized from the indexer; single embeds wrap the batch call.
/// </summary>
public sealed class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly OpenAiOptions _options;
    private readonly ILogger<OpenAiEmbeddingService> _logger;

    public OpenAiEmbeddingService(
        HttpClient httpClient,
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiEmbeddingService> logger)
    {
        _http = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured => _options.IsConfigured;

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var batch = await EmbedBatchAsync([text], cancellationToken);
        return batch[0];
    }

    public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new AiUnavailableException("The embedding provider is not configured.");
        }

        if (texts.Count == 0)
        {
            return [];
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "embeddings")
        {
            Content = JsonContent.Create(new
            {
                model = _options.EmbeddingModel,
                input = texts,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _http.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI embeddings request failed ({StatusCode}): {Body}",
                (int)response.StatusCode,
                Truncate(body));
            throw new InvalidOperationException(
                $"Embedding provider returned {(int)response.StatusCode}.");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.GetProperty("data")
            .EnumerateArray()
            .OrderBy(item => item.GetProperty("index").GetInt32())
            .Select(item => item.GetProperty("embedding")
                .EnumerateArray()
                .Select(value => (float)value.GetDouble())
                .ToArray())
            .ToList();
    }

    private static string Truncate(string value) =>
        value.Length <= 400 ? value : value[..400] + "…";
}