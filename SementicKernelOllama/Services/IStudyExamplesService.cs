namespace SementicKernelOllama.Services;

public interface IStudyExamplesService
{
    Task RunStudyExamplesAsync(CancellationToken cancellationToken = default);
    Task InteractiveStudySessionAsync(CancellationToken cancellationToken = default);
}