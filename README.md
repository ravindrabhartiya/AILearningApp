# AI Learning Hub

A comprehensive ASP.NET Core Blazor web application for learning Generative AI concepts, from fundamentals to advanced patterns, with hands-on labs using Azure OpenAI.

## Features

- **📚 6 Learning Modules** covering AI from novice to expert level:
  - Generative AI Foundations
  - Prompt Engineering
  - Retrieval Augmented Generation (RAG)
  - Model Fine-Tuning
  - AI Agents & Orchestration
  - Advanced Patterns & Production

- **🧪 Interactive Labs** with Azure OpenAI integration
- **📝 Quizzes** to test your understanding
- **📊 Progress Tracking** stored in browser local storage
- **🎨 Modern UI** with Bootstrap 5 and Bootstrap Icons

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Azure OpenAI Service](https://azure.microsoft.com/products/ai-services/openai-service) (for interactive labs)

## Getting Started

### 1. Clone and Navigate
```bash
cd "AI Learning App"
```

### 2. Configure Azure OpenAI (Optional but recommended for labs)

Update `appsettings.json` with your Azure OpenAI credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4",
    "ApiVersion": "2024-08-01-preview"
  }
}
```

Or use environment variables:
```bash
set AzureOpenAI__Endpoint=https://your-resource.openai.azure.com/
set AzureOpenAI__ApiKey=your-api-key-here
set AzureOpenAI__DeploymentName=gpt-4
```

Or use .NET User Secrets for development:
```bash
dotnet user-secrets init
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key-here"
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4"
```

### 3. Run the Application
```bash
dotnet run
```

### 4. Open in Browser
Navigate to `https://localhost:5001` or the URL shown in the terminal.

## Project Structure

```
AI Learning App/
├── Components/
│   ├── Layout/              # Main layout components
│   ├── Pages/               # Razor pages
│   │   ├── Home.razor       # Landing page
│   │   ├── Modules.razor    # Module listing
│   │   ├── Module.razor     # Module detail
│   │   ├── LearnLesson.razor # Lesson viewer
│   │   └── Settings.razor   # Configuration
│   ├── LabComponent.razor   # Interactive lab component
│   └── QuizComponent.razor  # Quiz component
├── Models/
│   ├── LearningModule.cs    # Module, Lesson, Lab, Quiz models
│   ├── UserProgress.cs      # Progress tracking models
│   └── AzureOpenAIModels.cs # API request/response models
├── Services/
│   ├── ContentService.cs    # Learning content management
│   ├── AzureOpenAIService.cs # Azure OpenAI integration
│   └── ProgressService.cs   # Progress tracking (local storage)
├── wwwroot/
│   └── app.css              # Custom styles
├── Program.cs               # Application startup
└── appsettings.json         # Configuration
```

## Learning Modules

| Module | Level | Topics |
|--------|-------|--------|
| Generative AI Foundations | Novice | LLM basics, architecture, first API call |
| Prompt Engineering | Beginner | Zero-shot, few-shot, chain-of-thought |
| RAG | Intermediate | Embeddings, vector databases, chunking |
| Fine-Tuning | Advanced | When to fine-tune, data preparation |
| AI Agents | Advanced | Tool calling, ReAct pattern, multi-agent |
| Advanced Patterns | Expert | Responsible AI, production deployment |

## Adding Custom Content

The learning content is defined in `Services/ContentService.cs`. You can:

1. **Add new lessons** to existing modules
2. **Create new modules** by adding methods to `InitializeLearningContent()`
3. **Customize labs** with different system prompts and parameters

## Technologies

- **ASP.NET Core 8.0** with Blazor Server
- **Bootstrap 5.3** for styling
- **Bootstrap Icons** for iconography
- **Azure OpenAI** for interactive labs
- **Local Storage** for progress persistence

## License

MIT License - feel free to use for learning and education purposes.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
