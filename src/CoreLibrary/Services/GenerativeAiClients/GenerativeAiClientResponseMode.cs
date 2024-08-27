namespace CoreLibrary.Services.GenerativeAiClients;

public enum GenerativeAiClientResponseMode
{
    /// <summary>
    /// Free text response, no constraints on the response's structure.
    /// </summary>
    PlainText,

    /// <summary>
    /// JSON Mode - a weak guarantee of response being in JSON format, but no schema guarantees.
    /// </summary>
    JsonMode,

    /// <summary>
    /// Strict JSON Mode (Structured Outputs) - a strong guarantee of response being in JSON format aligned with the provided schema.
    /// </summary>
    StructuredOutput
}
