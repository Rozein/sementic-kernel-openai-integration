using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using sementic_kernel_openai_integration;
using sementic_kernel_openai_integration.Plugins;
using sementic_kernel_openai_integration.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Services.Configure<OllamaSettings>(builder.Configuration.GetSection("Ollama"));


builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();
builder.Services.AddSingleton<IExampleService, ExampleService>();

builder.Services.AddSingleton<WeatherPlugin>();
builder.Services.AddSingleton<MathPlugin>();
builder.Services.AddSingleton<TextPlugin>();

var host = builder.Build();

try
{
    var examplesService = host.Services.GetRequiredService<IExampleService>();
    await examplesService.RunAllExamplesAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}