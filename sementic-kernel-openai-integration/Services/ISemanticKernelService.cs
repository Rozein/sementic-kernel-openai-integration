using Microsoft.SemanticKernel;

namespace sementic_kernel_openai_integration.Services;

public interface ISemanticKernelService
{
    Task InitializeAsync();
    Task<string> InvokePromptAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string> InvokePromptWithFunctionsAsync(string prompt, CancellationToken cancellationToken = default);
    Kernel GetKernel();
}