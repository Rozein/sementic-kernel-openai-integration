using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using sementic_kernel_openai_integration.Plugins;

namespace sementic_kernel_openai_integration.Services;

public class SemanticKernelService : ISemanticKernelService
{
    private readonly OllamaSettings _settings;
    private readonly WeatherPlugin _weatherPlugin;
    private readonly MathPlugin _mathPlugin;
    private readonly TextPlugin _textPlugin;
    private Kernel? _kernel;

    public SemanticKernelService(WeatherPlugin weatherPlugin, IOptions<OllamaSettings> settings, MathPlugin mathPlugin, TextPlugin textPlugin)
    {
        _weatherPlugin = weatherPlugin;
        _settings = settings.Value;
        _mathPlugin = mathPlugin;
        _textPlugin = textPlugin;
        
        Console.WriteLine($"Model ID: {_settings.ModelId}");
        Console.WriteLine($"Endpoint: {_settings.Endpoint}");
    }
    public async Task InitializeAsync()
    {
        try
        {
            Console.WriteLine("Initializing Semantic Kernel with Ollama...");

            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                modelId: _settings.ModelId,
                apiKey: "not-needed", 
                endpoint: new Uri(_settings.Endpoint + "/v1")); 


            _kernel = builder.Build();

            await RegisterPluginsAsync();
            
            Console.WriteLine("Semantic Kernel initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Semantic Kernel: {ex.Message}");
            throw;
        }
        
    }

    public async Task<string> InvokePromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var result = await _kernel.InvokePromptAsync(prompt, cancellationToken: cancellationToken);
                return result.ToString();
            }
            catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("quota"))
            {
                Console.WriteLine($"Rate limit hit (attempt {attempt}/{maxRetries}). Waiting {delay.TotalSeconds} seconds...");
                
                if (attempt == maxRetries)
                {
                    Console.WriteLine("❌ API quota exceeded. Please:");
                    Console.WriteLine("1. Check your OpenAI billing at https://platform.openai.com/account/billing");
                    Console.WriteLine("2. Add credits to your account");
                    Console.WriteLine("3. Verify your usage limits");
                    throw;
                }
                
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // Exponential backoff
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking prompt: {ex.Message}");
                throw;
            }
        }
        
        return string.Empty; 
    }

    public  async Task<string> InvokePromptWithFunctionsAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(_kernel);
        ArgumentException.ThrowIfNullOrWhiteSpace(prompt);

        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var settings = new OpenAIPromptExecutionSettings()
                {
                    Temperature = _settings.Temperature,
                    MaxTokens = _settings.MaxTokens
                };

                var result = await _kernel.InvokePromptAsync(prompt, new KernelArguments(settings), cancellationToken: cancellationToken);
                return result.ToString();
            }
            catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("quota"))
            {
                Console.WriteLine($"Rate limit hit (attempt {attempt}/{maxRetries}). Waiting {delay.TotalSeconds} seconds...");
                
                if (attempt == maxRetries)
                {
                    Console.WriteLine("❌ API quota exceeded. Please:");
                    Console.WriteLine("1. Check your OpenAI billing at https://platform.openai.com/account/billing");
                    Console.WriteLine("2. Add credits to your account");
                    Console.WriteLine("3. Verify your usage limits");
                    throw;
                }
                
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2); // Exponential backoff
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking prompt with functions: {ex.Message}");
                throw;
            }
        }
        
        return string.Empty;
    }

    public Kernel GetKernel()
    {
        ArgumentNullException.ThrowIfNull(_kernel);
        return _kernel;
    }
    private async Task RegisterPluginsAsync()
    {
        ArgumentNullException.ThrowIfNull(_kernel);

        Console.WriteLine("Registering plugins...");

        // Register plugins with dependency injection
        var weatherFunctions = _kernel.CreatePluginFromObject(_weatherPlugin, "Weather");
        _kernel.Plugins.Add(weatherFunctions);

        var mathFunctions = _kernel.CreatePluginFromObject(_mathPlugin, "Math");
        _kernel.Plugins.Add(mathFunctions);

        var textFunctions = _kernel.CreatePluginFromObject(_textPlugin, "Text");
        _kernel.Plugins.Add(textFunctions);

        Console.WriteLine("Plugins registered successfully");
        await Task.CompletedTask;
    }

}