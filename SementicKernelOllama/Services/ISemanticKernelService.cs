using Microsoft.SemanticKernel;
using sementic_kernel_openai_integration.Plugins;

namespace SementicKernelOllama.Services;

public interface ISemanticKernelService
{
    Task InitializeAsync();
    Task<string> InvokePromptAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> InvokePromptWithFunctionsAsync(string prompt, CancellationToken cancellationToken = default);
    Kernel GetKernel();
    void RegisterPDFStudyPlugin(PDFStudyPlugin pdfStudyPlugin);
    Task InvokePromptWithFunctionsStreamAsync(string prompt, CancellationToken cancellationToken = default);
}