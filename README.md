# Prodigee Tuts Point

Prodigee Tuts Point is a private, local-first learning web app for project-backed mastery across C#, ASP.NET Core, Swift, server-side Swift, TypeScript, Node.js, algorithms, and senior engineering practice.

## Development

Run the backend:

```bash
dotnet run --project src/ProdigeeTutsPoint.Api
```

Run the frontend:

```bash
cd src/ProdigeeTutsPoint.Web
npm run dev
```

Build the .NET solution:

```bash
dotnet build ProdigeeTutsPoint.slnx
```

Run .NET tests:

```bash
dotnet test ProdigeeTutsPoint.slnx
```

Build the frontend:

```bash
cd src/ProdigeeTutsPoint.Web
npm run build
```

## Local Secrets

Do not store secrets in the project directory or in .NET user-secrets. Use environment variables or an external local JSON file:

```text
~/.prodigee-tuts-point/secrets.json
```
