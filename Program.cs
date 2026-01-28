using AILearningApp.Components;
using AILearningApp.Models;
using AILearningApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure Azure OpenAI
builder.Services.Configure<AzureOpenAIConfig>(
    builder.Configuration.GetSection("AzureOpenAI"));

// Register application services
builder.Services.AddSingleton<IContentService, ContentService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddHttpClient<IAzureOpenAIService, AzureOpenAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
