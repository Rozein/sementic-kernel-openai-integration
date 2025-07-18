namespace SementicKernelOllama.Settings;

public class OllamaSettings
{
    public string ModelId { get; set; } 
    public string Endpoint { get; set; } 
    public int MaxTokens { get; set; } 
    public double Temperature { get; set; } 
}