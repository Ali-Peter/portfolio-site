using System.Text.RegularExpressions;

namespace Portfolio.Application.Common;

/// <summary>
/// Pragmatic text splitter used when building the RAG index: paragraphs are
/// packed into ~<c>targetLength</c>-character chunks; oversized paragraphs are
/// broken at sentence boundaries, with a fixed-window fallback for pathological
/// input (e.g. one enormous code block).
/// </summary>
public static partial class TextChunker
{
    private static readonly Regex SentenceBoundary = SentenceBoundaryRegex();

    /// <summary>Splits <paramref name="text"/> into embeddable chunks.</summary>
    public static IReadOnlyList<string> SplitToChunks(
        string text,
        int targetLength = 900,
        int maxLength = 1600)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var paragraphs = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SelectMany(paragraph => paragraph.Length > maxLength
                ? SplitOversizedParagraph(paragraph, maxLength)
                : [paragraph])
            .ToList();

        var chunks = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (current.Length > 0 && current.Length + paragraph.Length + 2 > targetLength)
            {
                chunks.Add(current.ToString());
                current.Clear();
            }

            if (current.Length > 0)
            {
                current.Append("\n\n");
            }

            current.Append(paragraph);
        }

        if (current.Length > 0)
        {
            chunks.Add(current.ToString());
        }

        return chunks;
    }

    /// <summary>
    /// Greedily packs sentences of an oversized paragraph into maxLength pieces.
    /// </summary>
    private static IEnumerable<string> SplitOversizedParagraph(string paragraph, int maxLength)
    {
        var sentences = SentenceBoundary
            .Split(paragraph)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0);

        var pieces = new List<string>();
        var current = new System.Text.StringBuilder();

        foreach (var sentence in sentences)
        {
            // A single sentence longer than maxLength: hard-split by characters.
            if (sentence.Length > maxLength)
            {
                if (current.Length > 0)
                {
                    pieces.Add(current.ToString());
                    current.Clear();
                }

                for (var offset = 0; offset < sentence.Length; offset += maxLength)
                {
                    pieces.Add(sentence.Substring(offset, Math.Min(maxLength, sentence.Length - offset)));
                }

                continue;
            }

            if (current.Length > 0 && current.Length + sentence.Length + 1 > maxLength)
            {
                pieces.Add(current.ToString());
                current.Clear();
            }

            if (current.Length > 0)
            {
                current.Append(' ');
            }

            current.Append(sentence);
        }

        if (current.Length > 0)
        {
            pieces.Add(current.ToString());
        }

        return pieces;
    }

    /// <summary>
    /// Lightweight Markdown cleanup for embedding: drops images and raw HTML,
    /// keeps headings/lists/code as plain text noise is acceptable for retrieval.
    /// </summary>
    public static string StripMarkdownDecorations(string markdown)
    {
        var text = HtmlTagRegex().Replace(markdown, " ");
        text = ImageSyntaxRegex().Replace(text, " ");
        text = WhitespaceRunsRegex().Replace(text, " ");
        return text.Trim();
    }

    [GeneratedRegex(@"!\[[^\]]*\]\([^)]*\)", RegexOptions.CultureInvariant)]
    private static partial Regex ImageSyntaxRegex();

    [GeneratedRegex(@"<[^>]+>", RegexOptions.CultureInvariant)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"[ \t]+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRunsRegex();

    [GeneratedRegex(@"(?<=[.!?])\s+", RegexOptions.CultureInvariant)]
    private static partial Regex SentenceBoundaryRegex();
}