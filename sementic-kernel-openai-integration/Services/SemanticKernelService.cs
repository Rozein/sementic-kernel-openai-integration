using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using sementic_kernel_openai_integration.Plugins;
using sementic_kernel_openai_integration.Settings;

namespace sementic_kernel_openai_integration.Services;

public class SemanticKernelService : ISemanticKernelService
{
    private readonly OllamaSettings _settings;
    private readonly WeatherPlugin _weatherPlugin;
    private readonly MathPlugin _mathPlugin;
    private readonly TextPlugin _textPlugin;
    private readonly HttpClient _httpClient;
    private Kernel? _kernel;
    private IChatCompletionService? _chatCompletionService;

    public SemanticKernelService(
        IOptions<OllamaSettings> settings,
        WeatherPlugin weatherPlugin,
        MathPlugin mathPlugin,
        TextPlugin textPlugin)
    {
        _settings = settings.Value;
        _weatherPlugin = weatherPlugin;
        _mathPlugin = mathPlugin;
        _textPlugin = textPlugin;
        _httpClient = new HttpClient();
        
        // Debug output
        Console.WriteLine($"Model ID: {_settings.ModelId}");
        Console.WriteLine($"Endpoint: {_settings.Endpoint}");
    }

    public async Task InitializeAsync()
    {
        try
        {
            Console.WriteLine("Initializing Semantic Kernel with Chat Completion Service...");

            // Test Ollama connection
            await TestOllamaConnectionAsync();

            // Create kernel with proper chat completion service
            var builder = Kernel.CreateBuilder();
            
            // Add OpenAI chat completion pointing to Ollama
            builder.AddOpenAIChatCompletion(
                modelId: _settings.ModelId,
                apiKey: "ollama-local", // Dummy key for local Ollama
                endpoint: new Uri($"{_settings.Endpoint}/v1")); // Ollama OpenAI-compatible endpoint

            _kernel = builder.Build();
            
            // Get the chat completion service
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            await RegisterPluginsAsync();
            
            Console.WriteLine("Semantic Kernel initialized successfully with Chat Completion");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Semantic Kernel: {ex.Message}");
            throw;
        }
    }

    private async Task TestOllamaConnectionAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_settings.Endpoint}/api/tags");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✅ Ollama connection successful");
            }
            else
            {
                throw new Exception($"Ollama connection failed: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Cannot connect to Ollama at {_settings.Endpoint}. Make sure 'ollama serve' is running. Error: {ex.Message}");
        }
    }

    private async Task RegisterPluginsAsync()
    {
        ArgumentNullException.ThrowIfNull(_kernel);

        Console.WriteLine("Registering plugins...");

        // Register basic plugins
        var weatherFunctions = _kernel.CreatePluginFromObject(_weatherPlugin, "Weather");
        _kernel.Plugins.Add(weatherFunctions);

        var mathFunctions = _kernel.CreatePluginFromObject(_mathPlugin, "Math");
        _kernel.Plugins.Add(mathFunctions);

        var textFunctions = _kernel.CreatePluginFromObject(_textPlugin, "Text");
        _kernel.Plugins.Add(textFunctions);

        Console.WriteLine("Basic plugins registered successfully (Weather, Math, Text)");
        await Task.CompletedTask;
    }

    public async Task<string> InvokePromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_chatCompletionService);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        try
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            var settings = new OpenAIPromptExecutionSettings()
            {
                Temperature = _settings.Temperature,
                MaxTokens = _settings.MaxTokens
            };

            var result = await _chatCompletionService.GetChatMessageContentsAsync(
                chatHistory, 
                settings, 
                _kernel, 
                cancellationToken);

            return result[0].Content ?? "No response";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking prompt: {ex.Message}");
            throw;
        }
    }

    public async Task<string> InvokePromptWithFunctionsAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_chatCompletionService);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        try
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            var settings = new OpenAIPromptExecutionSettings()
            {
                Temperature = _settings.Temperature,
                MaxTokens = _settings.MaxTokens,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var result = await _chatCompletionService.GetChatMessageContentsAsync(
                chatHistory, 
                settings, 
                _kernel, 
                cancellationToken);

            return result[0].Content ?? "No response";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking prompt with functions: {ex.Message}");
            throw;
        }
    }

    public async Task InvokePromptWithFunctionsStreamAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_chatCompletionService);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        try
        {
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            var settings = new OpenAIPromptExecutionSettings()
            {
                Temperature = _settings.Temperature,
                MaxTokens = _settings.MaxTokens,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            Console.Write("AI: ");

            await foreach (var update in _chatCompletionService.GetStreamingChatMessageContentsAsync(
                chatHistory, 
                settings, 
                _kernel, 
                cancellationToken))
            {
                if (update.Content != null)
                {
                    Console.Write(update.Content);
                    await Task.Delay(50, cancellationToken); // Small delay for readability
                }
            }

            Console.WriteLine(); // New line after streaming
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error invoking prompt with functions (stream): {ex.Message}");
            throw;
        }
    }

    public Kernel GetKernel()
    {
        ArgumentNullException.ThrowIfNull(_kernel);
        return _kernel;
    }
    public void RegisterPDFStudyPlugin(PDFStudyPlugin pdfStudyPlugin)
    {
        ArgumentNullException.ThrowIfNull(_kernel);
        
        try
        {
            var pdfStudyFunctions = _kernel.CreatePluginFromObject(pdfStudyPlugin, "PDFStudy");
            _kernel.Plugins.Add(pdfStudyFunctions);
            Console.WriteLine("PDFStudy plugin registered successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error registering PDFStudy plugin: {ex.Message}");
        }
    }
}