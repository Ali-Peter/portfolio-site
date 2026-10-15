namespace Portfolio.Application.Common;

/// <summary>
/// Tuning knobs for the RAG assistant. Bound from the "Ai" configuration
/// section in the API composition root; the defaults are safe for public use.
/// </summary>
public sealed class AiAssistantOptions
{
    public const string SectionName = "Ai";

    /// <summary>Number of nearest chunks retrieved per question.</summary>
    public int TopK { get; set; } = 6;

    /// <summary>Maximum accepted question length in characters (input limit).</summary>
    public int MaxQuestionChars { get; set; } = 2000;

    /// <summary>Hard cap on the total context text sent to the model.</summary>
    public int MaxContextChars { get; set; } = 9000;
}