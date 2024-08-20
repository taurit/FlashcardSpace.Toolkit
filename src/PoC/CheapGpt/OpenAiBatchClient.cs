using CheapGpt.Models;
using OpenAI.Batch;
using OpenAI.Files;
using System.ClientModel;
using System.Text.Json;

namespace CheapGpt;

/// <summary>
/// A proof-of-concept abstraction layer over OpenAI Batch & Files API, allowing to use Batch API as if it was a synchronous service
/// (even though it might take 24 hours to retrieve a response).
///
/// It maintains a filesystem-backed state of the batch job, so that the client can be restarted and the job can be resumed.
/// </summary>
public class OpenAiBatchClient(string openAiDeveloperKey, string modelId, int maxTokensPerPrompt, string internalStateFolderPath)
{
    private readonly List<string> _prompts = new();

    public void AddPrompt(string prompt)
    {
        if (_prompts.Any(x => x == prompt)) return; // already added
        _prompts.Add(prompt);
    }


    public async Task StartJob()
    {
        // Create folder and file structure to keep job state persisted
        string jobId = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}";
        var jobFolderPath = Path.Combine(internalStateFolderPath, jobId);
        Directory.CreateDirectory(jobFolderPath);

        var promptsRootFolderPath = Path.Combine(jobFolderPath, "prompts");
        Directory.CreateDirectory(promptsRootFolderPath);

        var batchJobInputFilePath = Path.Combine(jobFolderPath, "input.jsonl");

        var lines = new List<string>();
        foreach (var prompt in _prompts)
        {
            var promptModel = new PromptModel(prompt, null);
            var promptModelSerialized = JsonSerializer.Serialize(promptModel);

            var promptFileName = GeneratePromptFileName(prompt);
            var promptFilePath = Path.Combine(promptsRootFolderPath, promptFileName);

            await File.WriteAllTextAsync(promptFilePath, promptModelSerialized);

            // Create input file for the batch job
            var bodyModel = new OpenAiBatchJobBody(modelId, maxTokensPerPrompt, new OpenAiBatchJobBodyMessages[] {
                new OpenAiBatchJobBodyMessages("system", "You are a helpful assistant."),
                new OpenAiBatchJobBodyMessages("user", prompt)
            });

            var batchLineForPromptModel = new OpenAiBatchJobLine(promptFileName, bodyModel);
            var batchLineForPromptModelSerialized = JsonSerializer.Serialize(batchLineForPromptModel);
            // may be needed, but test first:
            //batchLineForPromptModelSerialized = batchLineForPromptModelSerialized.Replace(Environment.NewLine, "\\n");

            lines.Add(batchLineForPromptModelSerialized);
        }
        await File.WriteAllLinesAsync(batchJobInputFilePath, lines);

        // Upload the file with prompts to OpenAI Files API
        var fileId = await UploadFileToOpenAiStorage(batchJobInputFilePath);

        await CreateAndStartBatchJob(fileId);

    }

    private async Task<string> UploadFileToOpenAiStorage(string localFileName)
    {
        var apiKeyCredential = new ApiKeyCredential(openAiDeveloperKey);
        FileClient fileClient = new(apiKeyCredential);

        var response = await fileClient.UploadFileAsync(localFileName, FileUploadPurpose.Batch);
        // todo handle errors

        return response.Value.Id;
    }

    private string GeneratePromptFileName(string prompt)
    {
        return $"{modelId}-{prompt.GetHashCodeStable()}.json";
    }

    public async Task CreateAndStartBatchJob(string fileId)
    {
        // Create a client for the OpenAI Batch API
        var apiKeyCredential = new ApiKeyCredential(openAiDeveloperKey);
        BatchClient batchClient = new(apiKeyCredential);

        var batchRequest = new BatchJobCreationRequestModel(fileId);
        var response = await batchClient.CreateBatchAsync(BinaryContent.Create(BinaryData.FromObjectAsJson(batchRequest)));
        var rawResponse = response.GetRawResponse();

        Console.WriteLine(rawResponse.Content);
    }

    public async Task<string> GetAnswer(string prompt)
    {
        // todo implement
        return "";
    }
}
