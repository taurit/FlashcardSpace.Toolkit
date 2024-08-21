# Secrets

This document explains how a user of the `generateflashcards.exe` tool can pass secrets to the application. See the relevant [Architecture Decision Record](Architecture%20Decision%20Records/2024-08-21%20How%20users%20should%20pass%20secret%20parameters.md) to understand why secrets are passed differently than all other parameters.
 
### Setting secrets via Environment Variables

The application accepts secrets via the following environment variables:

```.env
OPENAI_ORGANIZATION_ID=...
OPENAI_DEVELOPER_KEY=...
```

### Secrets File

If you use the `--secretsFile` parameter to declare the secrets to use, the file should have the following structure:

```json
{
  "OPENAI_ORGANIZATION_ID": "...",
  "OPENAI_DEVELOPER_KEY": "..."
}
```

### Visual Studio Secret Manager

In the case of VS Secret Manager, right-click on the project in Solution Explorer, select "Manage User Secrets," and use the JSON structure described above in the **Secrets File** section.