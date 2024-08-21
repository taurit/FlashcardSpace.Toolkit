using Microsoft.Extensions.Configuration;

namespace BookToAnki.Tests;

internal record UserSecrets(string OpenAiDeveloperKey, string OpenAiOrganizationId);

internal static class UserSecretsRetriever
{
    internal static UserSecrets GetUserSecrets()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<UserSecrets>();
        var configuration = builder.Build();

        var devKey = configuration["OPENAI_DEVELOPER_KEY"] ??
                             throw new InvalidOperationException(
                                 "OPENAI_DEVELOPER_KEY is missing in User Secrets configuration");
        var organizationId = configuration["OPENAI_ORGANIZATION_ID"] ??
                             throw new InvalidOperationException(
                                 "OPENAI_ORGANIZATION_ID is missing in User Secrets configuration");

        return new UserSecrets(devKey, organizationId);
    }
}
