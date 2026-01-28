# AI Learning Hub - Development Guidelines

## Project Overview
This is an ASP.NET Core Blazor Server application for learning Generative AI concepts with Azure OpenAI integration.

## Architecture

### Models (`/Models`)
- `LearningModule.cs` - Core learning content models (Module, Lesson, Lab, Quiz)
- `UserProgress.cs` - Progress tracking models
- `AzureOpenAIModels.cs` - Azure OpenAI API models

### Services (`/Services`)
- `ContentService` - Manages static learning content (singleton)
- `AzureOpenAIService` - Handles Azure OpenAI API calls (HttpClient)
- `ProgressService` - Manages user progress via browser local storage (scoped)

### Components (`/Components`)
- Pages are in `/Components/Pages`
- Shared components like `LabComponent` and `QuizComponent` are in `/Components`

## Development Guidelines

### Adding New Content
1. Open `Services/ContentService.cs`
2. Add new lessons to existing modules or create new modules
3. Follow the existing pattern for content sections, labs, and quizzes

### Azure OpenAI Configuration
Configure in `appsettings.json` or environment variables:
- `AzureOpenAI:Endpoint` - Your Azure OpenAI resource endpoint
- `AzureOpenAI:ApiKey` - Your API key
- `AzureOpenAI:DeploymentName` - Your model deployment name
- `AzureOpenAI:ApiVersion` - API version (default: 2024-08-01-preview)

### Running the Application
```bash
dotnet run
```

### Building for Production
```bash
dotnet publish -c Release
```

## Key Features
- Interactive labs with real Azure OpenAI API calls
- Progress tracking via browser local storage
- Markdown content rendering in lessons
- Quiz system with scoring and explanations
