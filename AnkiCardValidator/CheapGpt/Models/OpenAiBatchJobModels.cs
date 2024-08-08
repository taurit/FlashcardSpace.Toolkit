// ReSharper disable InconsistentNaming -> Names must match the specification; avoiding JsonPropertyName for brevity

namespace CheapGpt;

record OpenAiBatchJobLine(string custom_id, OpenAiBatchJobBody body)
{
    public string method => "POST";
    public string url => "/v1/chat/completions";
}
record OpenAiBatchJobBody(string model, int max_tokens, OpenAiBatchJobBodyMessages[] messages);
record OpenAiBatchJobBodyMessages(string role, string content);

