using Microsoft.SemanticKernel;
using sementic_kernel_openai_integration.Plugins;

namespace sementic_kernel_openai_integration.Services;

public interface ISemanticKernelService
{
    Task InitializeAsync();
    Task<string> InvokePromptAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> InvokePromptWithFunctionsAsync(string prompt, CancellationToken cancellationToken = default);
    Kernel GetKernel();
    void RegisterPDFStudyPlugin(PDFStudyPlugin pdfStudyPlugin);

}