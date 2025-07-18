using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace sementic_kernel_openai_integration.Plugins;

public class MathPlugin
{
    [KernelFunction, Description("Add two numbers")]
    public double Add(double a, double b)
    {
        var result = a + b;
        Console.WriteLine($"Adding {a} + {b} = {result}");
        return result;
    }

    [KernelFunction, Description("Subtract two numbers")]
    public double Subtract(double a, double b)
    {
        var result = a - b;
        Console.WriteLine($"Subtracting {a} - {b} = {result}");
        return result;
    }

    [KernelFunction, Description("Multiply two numbers")]
    public double Multiply(double a, double b)
    {
        var result = a * b;
        Console.WriteLine($"Multiplying {a} * {b} = {result}");
        return result;
    }

    [KernelFunction, Description("Divide two numbers")]
    public double Divide(double a, double b)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(b, 0.0, nameof(b));
        
        var result = a / b;
        Console.WriteLine($"Dividing {a} / {b} = {result}");
        return result;
    }

    [KernelFunction, Description("Calculate power of a number")]
    public double Power(double baseNumber, double exponent)
    {
        var result = Math.Pow(baseNumber, exponent);
        Console.WriteLine($"Calculating {baseNumber}^{exponent} = {result}");
        return result;
    }
}