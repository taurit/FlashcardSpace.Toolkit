# Secrets

This document explains how a user of the `generateflashcards.exe` tool can pass secrets to the application. See the relevant [Architecture Decision Record](Architecture%20Decision%20Records/2024-08-21%20How%20users%20should%20pass%20secret%20parameters.md) to understand why secrets are passed differently than all other parameters.

The application will attempt to read secrets from the sources in this order:

### Visual Studio Secret Manager secrets

To define the secrets, in Visual Studio, right-click on the project in the Solution Explorer, select "Manage User Secrets," and use the JSON structure like:

```json
{
    "OPENAI_ORGANIZATION_ID": "...",
    "OPENAI_DEVELOPER_KEY": "..."
}
```


### The secrets.json file

Alternatively, you can place secrets in the file named `secrets.json` in program's working directory. The file should have the following structure:

```json
{
  "OPENAI_ORGANIZATION_ID": "...",
  "OPENAI_DEVELOPER_KEY": "..."
}
```

### Secrets in Environment Variables

The application also accepts secrets via the following environment variables:

```.env
OPENAI_ORGANIZATION_ID=...
OPENAI_DEVELOPER_KEY=...
```

For example, in Windows command prompt, you could use:

```cmd
set OPENAI_ORGANIZATION_ID=mySecret1
set OPENAI_DEVELOPER_KEY=mySecret2

GenerateFlashcards.exe --yourUsualParameters ...
```
