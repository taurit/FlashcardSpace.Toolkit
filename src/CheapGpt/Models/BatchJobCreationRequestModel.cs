// ReSharper disable InconsistentNaming
namespace CheapGpt.Models;

public record BatchJobCreationRequestModel(string input_file_id)
{
    public string endpoint => "/v1/chat/completions";
    public string completion_window => "24h";
}
