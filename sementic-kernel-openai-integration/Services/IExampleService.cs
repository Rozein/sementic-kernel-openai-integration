namespace sementic_kernel_openai_integration.Services;

public interface IExampleService
{
    Task RunAllExamplesAsync(CancellationToken cancellationToken = default);
    Task SimpleTextCompletionExample(CancellationToken cancellationToken = default);
    Task WeatherFunctionExample(CancellationToken cancellationToken = default);
    Task MathFunctionExample(CancellationToken cancellationToken = default);
    Task TextProcessingExample(CancellationToken cancellationToken = default);
    Task ConversationExample(CancellationToken cancellationToken = default);

}