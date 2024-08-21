using Microsoft.Extensions.Configuration;

namespace BookToAnki.Tests;

internal record UserSecrets(string OpenAiDeveloperKey, string OpenAiOrganization);

internal static class UserSecretsRetriever
{
    internal static UserSecrets GetUserSecrets()
    {
        var builder = new ConfigurationBuilder().AddUserSecrets<UserSecrets>();
        var configuration = builder.Build();

        var devKey = configuration["OpenAiDeveloperKey"] ??
                             throw new InvalidOperationException(
                                 "OpenAiDeveloperKey is missing in User Secrets configuration");
        var organization = configuration["OpenAiOrganization"] ??
                             throw new InvalidOperationException(
                                 "OpenAiOrganization is missing in User Secrets configuration");

        return new UserSecrets(devKey, organization);
    }
}
