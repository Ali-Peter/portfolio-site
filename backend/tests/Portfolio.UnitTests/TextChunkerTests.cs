using Portfolio.Application.Common;

namespace Portfolio.UnitTests;

/// <summary>
/// Chunk splitting used when building the RAG index: chunks must stay within
/// the embedding model's practical window and preserve their source text.
/// </summary>
public sealed class TextChunkerTests
{
    [Fact]
    public void ShortText_IsSingleChunk()
    {
        var chunks = TextChunker.SplitToChunks("One short paragraph about the portfolio.");

        Assert.Single(chunks);
        Assert.Equal("One short paragraph about the portfolio.", chunks[0]);
    }

    [Fact]
    public void EmptyText_ReturnsNoChunks()
    {
        Assert.Empty(TextChunker.SplitToChunks("   "));
        Assert.Empty(TextChunker.SplitToChunks(string.Empty));
    }

    [Fact]
    public void LongText_PacksParagraphsWithinTargetLength()
    {
        var paragraph = string.Join(" ", Enumerable.Range(0, 40).Select(_ => "word"));
        var text = string.Join("\n\n", Enumerable.Range(0, 10).Select(_ => paragraph));

        var chunks = TextChunker.SplitToChunks(text, targetLength: 500, maxLength: 900);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, chunk => Assert.True(chunk.Length <= 900));
        // Nothing lost: every paragraph survives into some chunk.
        Assert.Equal(10, chunks.Sum(chunk => chunk.Split("word").Length - 1) / 40);
    }

    [Fact]
    public void OversizedParagraph_IsSplitAtSentenceBoundaries()
    {
        var sentence = "This is a perfectly reasonable sentence about enterprise software. ";
        var text = string.Concat(Enumerable.Range(0, 80).Select(_ => sentence));

        var chunks = TextChunker.SplitToChunks(text, targetLength: 500, maxLength: 600);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, chunk => Assert.True(chunk.Length <= 600));
    }

    [Fact]
    public void StripMarkdownDecorations_RemovesImagesAndHtml()
    {
        var markdown = """
            # Heading

            Some text with ![a picture](/assets/x.png) inline and <b>tags</b> here.

            [A link](https://example.com) stays as text.
            """;

        var cleaned = TextChunker.StripMarkdownDecorations(markdown);

        Assert.DoesNotContain("![", cleaned);
        Assert.DoesNotContain("<b>", cleaned);
        Assert.Contains("Some text", cleaned);
        Assert.Contains("A link", cleaned);
    }
}