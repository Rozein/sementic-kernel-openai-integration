namespace sementic_kernel_openai_integration.Services;

public class ExampleService : IExampleService
{
    private readonly ISemanticKernelService _kernelService;

    public ExampleService(ISemanticKernelService kernelService)
    {
        _kernelService = kernelService;
    }

    public async Task RunAllExamplesAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Initializing Semantic Kernel...");
        await _kernelService.InitializeAsync();

        Console.WriteLine("Running all examples with delays to avoid rate limits...");

        await SimpleTextCompletionExample(cancellationToken);
        await Task.Delay(2000, cancellationToken); // 2 second delay

        await WeatherFunctionExample(cancellationToken);
        await Task.Delay(2000, cancellationToken);

        await MathFunctionExample(cancellationToken);
        await Task.Delay(2000, cancellationToken);

        await TextProcessingExample(cancellationToken);
        await Task.Delay(2000, cancellationToken);

        await ConversationExample(cancellationToken);

        Console.WriteLine("All examples completed");
    }

    public async Task SimpleTextCompletionExample(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== Simple Text Completion ===");
        
        const string prompt = "Explain artificial intelligence in simple terms";
        var response = await _kernelService.InvokePromptAsync(prompt, cancellationToken);
        Console.WriteLine(response);
        Console.WriteLine();
    }

    public async Task WeatherFunctionExample(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== Weather Function Example ===");
        
        const string prompt = """
            The user wants to know about weather. 
            User question: What's the weather like in London and Tokyo?
            
            Use the Weather plugin GetWeather function to help answer for both cities.
            """;

        var response = await _kernelService.InvokePromptWithFunctionsAsync(prompt, cancellationToken);
        Console.WriteLine(response);
        Console.WriteLine();
    }

    public async Task MathFunctionExample(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== Math Function Example ===");
        
        const string prompt = """
            Solve this math problem step by step:
            Calculate: (25 * 4) + (100 - 37) - (15 * 2)
            
            Use the Math plugin functions to help with calculations.
            """;

        var response = await _kernelService.InvokePromptWithFunctionsAsync(prompt, cancellationToken);
        Console.WriteLine(response);
        Console.WriteLine();
    }

    public async Task TextProcessingExample(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== Text Processing Example ===");
        
        const string prompt = """
            Take this text and make it more professional:
            "hey there, i hope ur doing good. we need to talk about the project asap"
            
            Use the Text plugin ImproveText function to help.
            """;

        var response = await _kernelService.InvokePromptWithFunctionsAsync(prompt, cancellationToken);
        Console.WriteLine(response);
        Console.WriteLine();
    }

    public async Task ConversationExample(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== Interactive Conversation ===");
        Console.WriteLine("Ask me anything (type 'quit' to exit):");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("You: ");
            var userInput = Console.ReadLine();
            
            if (string.IsNullOrEmpty(userInput) || userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            try
            {
                var response = await _kernelService.InvokePromptWithFunctionsAsync(userInput, cancellationToken);
                Console.WriteLine($"AI: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in conversation: {ex.Message}");
            }
        }
    }
}