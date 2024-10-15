namespace CoreLibrary.Services.ObjectGenerativeFill;

/// <summary>
/// Exception thrown when the response from the AI model does not match the expected format, e.g.:
/// - the number of elements in output is different from the number of elements in input
/// - deserialization fails
/// </summary>
public class ResponseMismatchException : Exception
{
    public ResponseMismatchException(string message) : base(message)
    {
    }
}
