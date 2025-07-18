// Program.cs - Complete Final Version
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using sementic_kernel_openai_integration.Plugins;
using SementicKernelOllama.Services;
using SementicKernelOllama.Settings;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// Configure settings
builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("Ollama"));

// Register plugins first (no dependencies)
builder.Services.AddSingleton<WeatherPlugin>();
builder.Services.AddSingleton<MathPlugin>();
builder.Services.AddSingleton<TextPlugin>();

// Register core services (PDF processing)
builder.Services.AddSingleton<IPDFService, PDFService>();

// Register kernel service (depends on basic plugins only)
builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();

// Register study service (depends on kernel and PDF services)
builder.Services.AddSingleton<IStudyService, StudyService>();

// Register PDF study plugin (depends on study service)
builder.Services.AddSingleton<PDFStudyPlugin>();

// Register example services
builder.Services.AddSingleton<IExampleService, ExampleService>();
builder.Services.AddSingleton<IStudyExamplesService, StudyExamplesService>();

var host = builder.Build();

try
{
    Console.WriteLine("🎓 Welcome to PDF Study Assistant with Semantic Kernel!");
    Console.WriteLine("Powered by Ollama and your M3 Pro Mac");
    Console.WriteLine();
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. Run basic examples (Weather, Math, Text)");
    Console.WriteLine("2. PDF Study examples (Demo mode)");
    Console.WriteLine("3. Interactive PDF study session");
    Console.WriteLine("4. Exit");
    Console.Write("Enter choice (1-4): ");
    
    var choice = Console.ReadLine();
    
    switch (choice)
    {
        case "1":
            Console.WriteLine("\n🔧 Running basic Semantic Kernel examples...");
            var examplesService = host.Services.GetRequiredService<IExampleService>();
            await examplesService.RunAllExamplesAsync();
            break;
            
        case "2":
            Console.WriteLine("\n📚 Running PDF study examples...");
            var studyExamplesService = host.Services.GetRequiredService<IStudyExamplesService>();
            await studyExamplesService.RunStudyExamplesAsync();
            break;
            
        case "3":
            Console.WriteLine("\n🎯 Starting interactive PDF study session...");
            Console.WriteLine("You can now load PDF files and start studying!");
            var interactiveService = host.Services.GetRequiredService<IStudyExamplesService>();
            await interactiveService.InteractiveStudySessionAsync();
            break;
            
        case "4":
            Console.WriteLine("👋 Goodbye! Happy studying!");
            return;
            
        default:
            Console.WriteLine("Invalid choice. Running basic examples...");
            var defaultService = host.Services.GetRequiredService<IExampleService>();
            await defaultService.RunAllExamplesAsync();
            break;
    }
    
    Console.WriteLine("\n✅ Session completed!");
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine("\nTroubleshooting tips:");
    Console.WriteLine("1. Make sure Ollama is running: ollama serve");
    Console.WriteLine("2. Check if you have the model: ollama list");
    Console.WriteLine("3. Download model if needed: ollama pull qwen2.5:14b");
    Console.WriteLine("4. Verify appsettings.json configuration");
}