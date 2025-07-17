using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace sementic_kernel_openai_integration.Plugins;

public class TextPlugin
{
    [KernelFunction, Description("Improve text to make it more professional")]
    public string ImproveText(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        // Simple text improvements
        var improved = text
            .Replace("ur", "you are")
            .Replace("u", "you")
            .Replace("asap", "as soon as possible")
            .Replace("hey there", "Hello")
            .Replace("i hope", "I hope")
            .Replace("we need to", "We need to");

        // Capitalize first letter
        if (!string.IsNullOrEmpty(improved))
        {
            improved = char.ToUpper(improved[0]) + improved.Substring(1);
        }

        // Add period if missing
        if (!improved.EndsWith('.') && !improved.EndsWith('!') && !improved.EndsWith('?'))
        {
            improved += ".";
        }

        return improved;
    }
    [KernelFunction, Description("Count words in text")]
    public int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;
        
        var words = text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        return words.Length;
    }
    [KernelFunction, Description("Summarize text")]
    public string SummarizeText(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        
        var sentences = text.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (sentences.Length <= 2)
            return text;
        
        // Take first and last sentences as summary
        return $"{sentences[0].Trim()}. {sentences[^1].Trim()}.";
    }
}