using Microsoft.Extensions.DependencyInjection;
using sementic_kernel_openai_integration.Plugins;

namespace sementic_kernel_openai_integration.Services;

public class StudyExamplesService : IStudyExamplesService
{
    private readonly ISemanticKernelService _kernelService;
    private readonly IServiceProvider _serviceProvider;

    public StudyExamplesService(ISemanticKernelService kernelService, IServiceProvider serviceProvider)
    {
        _kernelService = kernelService;
        _serviceProvider = serviceProvider;
    }

    public async Task RunStudyExamplesAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== PDF Study Assistant Examples ===");
        Console.WriteLine("Note: These examples demonstrate the PDF study plugin capabilities");
        Console.WriteLine();

        await _kernelService.InitializeAsync();
        
        // Register PDF plugin after kernel initialization
        var pdfPlugin = _serviceProvider.GetRequiredService<PDFStudyPlugin>();
        _kernelService.RegisterPDFStudyPlugin(pdfPlugin);
        
        await ShowAvailableFunctions();
        await ShowExampleUsage();
    }

    public async Task InteractiveStudySessionAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== Interactive PDF Study Session ===");
        Console.WriteLine("Commands:");
        Console.WriteLine("  load <path> <subject> - Load a PDF document");
        Console.WriteLine("  quiz <number> - Generate quiz questions");
        Console.WriteLine("  ask <question> - Ask a question about the document");
        Console.WriteLine("  summary - Get document summary");
        Console.WriteLine("  topics - Get study topics");
        Console.WriteLine("  explain <concept> - Explain a concept");
        Console.WriteLine("  help - Show this help");
        Console.WriteLine("  quit - Exit");
        Console.WriteLine();

        await _kernelService.InitializeAsync();
        
        // Register PDF plugin after kernel initialization
        var pdfPlugin = _serviceProvider.GetRequiredService<PDFStudyPlugin>();
        _kernelService.RegisterPDFStudyPlugin(pdfPlugin);

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write("Study> ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                await ShowDetailedHelp();
                continue;
            }

            try
            {
                var response = await _kernelService.InvokePromptWithFunctionsAsync(
                    $"Handle this study command: {input}. Use the appropriate PDF study functions.", cancellationToken);
                Console.WriteLine(response);
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
            }
        }
    }

    private async Task ShowAvailableFunctions()
    {
        Console.WriteLine("--- Available Study Functions ---");
        Console.WriteLine("✅ LoadPDFDocument - Load and process PDF files");
        Console.WriteLine("✅ GenerateQuizQuestions - Create MCQ from PDF content");
        Console.WriteLine("✅ AnswerFromDocument - Q&A based on PDF only");
        Console.WriteLine("✅ SummarizeDocument - Get document overview");
        Console.WriteLine("✅ GetStudyTopics - Extract key study areas");
        Console.WriteLine("✅ ExplainConcept - Detailed concept explanations");
        Console.WriteLine();
        
        await Task.CompletedTask;
    }

    private async Task ShowExampleUsage()
    {
        Console.WriteLine("--- Example Usage ---");
        Console.WriteLine("1. Load a document:");
        Console.WriteLine("   load /path/to/textbook.pdf \"Computer Science\"");
        Console.WriteLine();
        
        Console.WriteLine("2. Generate study questions:");
        Console.WriteLine("   Generate 5 quiz questions from the loaded document");
        Console.WriteLine();
        
        Console.WriteLine("3. Ask specific questions:");
        Console.WriteLine("   ask \"What are the main data structures discussed?\"");
        Console.WriteLine();
        
        Console.WriteLine("4. Get study overview:");
        Console.WriteLine("   summary");
        Console.WriteLine("   topics");
        Console.WriteLine();
        
        Console.WriteLine("5. Explain concepts:");
        Console.WriteLine("   explain \"binary search tree\"");
        Console.WriteLine();
        
        await Task.CompletedTask;
    }

    private async Task ShowDetailedHelp()
    {
        Console.WriteLine("=== Detailed Help ===");
        Console.WriteLine();
        
        Console.WriteLine("📁 LOADING DOCUMENTS:");
        Console.WriteLine("  load <pdf_path> <subject>");
        Console.WriteLine("  Example: load /Users/student/chapter1.pdf \"Mathematics\"");
        Console.WriteLine();
        
        Console.WriteLine("❓ GENERATING QUESTIONS:");
        Console.WriteLine("  quiz <number>");
        Console.WriteLine("  Example: quiz 10");
        Console.WriteLine("  Generates multiple choice questions from your PDF");
        Console.WriteLine();
        
        Console.WriteLine("💬 ASKING QUESTIONS:");
        Console.WriteLine("  ask \"your question here\"");
        Console.WriteLine("  Example: ask \"What is machine learning?\"");
        Console.WriteLine("  Answers will be based ONLY on your uploaded document");
        Console.WriteLine();
        
        Console.WriteLine("📋 STUDY TOOLS:");
        Console.WriteLine("  summary - Get a concise overview of the document");
        Console.WriteLine("  topics - List key study topics from the document");
        Console.WriteLine("  explain \"concept\" - Get detailed explanation of a concept");
        Console.WriteLine();
        
        Console.WriteLine("🎯 TIPS FOR BEST RESULTS:");
        Console.WriteLine("  • Use clear, text-based PDF files");
        Console.WriteLine("  • Ask specific questions about the content");
        Console.WriteLine("  • Start with 'summary' and 'topics' for overview");
        Console.WriteLine("  • Use 'explain' for difficult concepts");
        Console.WriteLine();
        
        await Task.CompletedTask;
    }
}