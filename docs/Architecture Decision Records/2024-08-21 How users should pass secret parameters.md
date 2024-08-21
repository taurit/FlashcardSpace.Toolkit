# Architecture Decision Record: How users should pass secret parameters?

## Problem Statement

The tool generating flashcard is generally intended to be used as a CLI app, so it's launched with a command like:

```cmd
GenerateFlashcards.exe --outputLanguage English input.txt
```

To use the Generative AI features, the application needs API keys for online services like OpenAI API.

Such keys are considered **secrets**, just like passwords. This is because if they are compromised, someone could use them e.g., to use the service at a cost of the key owner.

In theory, the application could accept them just like every other parameter:

```cmd
GenerateFlashcards.exe --openApiDeveloperKey SOME_SECRET_VALUE ... input.txt
```

However, this is not the best security practice (e.g., command parameters can remain persisted in terminal's command history), so the app should at least offer alternative options.

## Options considered

### Visual Studio Secret Manager ("Manage User Secrets")

For developers who launch the application from Visual Studio, the most convenient option seems to use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows).

Secret manager allows managing secrets needed by application in the IDE, and mitigates the risk that they would be accidentally committed to the git repository or backed up to an untrusted location along with the code. 

### Accepting a path to an external file with secrets 

Another option is to accept parameter like:

```cmd
GenerateFlashcards.exe --secretsFile secrets.json ... input.txt
```

The risk here is lower than if we accepted secrets directly because the access to the file can be limited and managed.

Also, it seems much more convenient and readable to not pass long secret strings every time we run the tool.

### Accepting secrets from environment variables

Another common place where an app can look for secret values is environment variables.

User could launch the tool with a command like:

Windows command prompt:
```cmd
set SECRET1=mySecret1 && set SECRET2=mySecret2 && GenerateFlashcards.exe
```
 
## Decision and rationale

The app will enable multiple ways of passing secrets (motivated mainly by convenience and variety of user's preferences).

The order of precedence will be:

1) Environment variables (override any other source)
2) The file declared via `--secretsFile` parameter
3) Visual Studio Secret Manager

See the ["Secrets"](../Secrets.md) article in the user documentation for the concrete names of secrets accepted by the application. 
