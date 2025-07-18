namespace sementic_kernel_openai_integration.Services;

public interface IStudyExamplesService
{
    Task RunStudyExamplesAsync(CancellationToken cancellationToken = default);
    Task InteractiveStudySessionAsync(CancellationToken cancellationToken = default);
}