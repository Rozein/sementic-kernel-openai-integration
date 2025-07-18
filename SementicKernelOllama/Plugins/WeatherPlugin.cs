using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace sementic_kernel_openai_integration.Plugins;

public class WeatherPlugin
{
    [KernelFunction, Description("Get current weather for a location")]
    public async Task<string> GetWeatherAsync(string location)
    {
        Console.WriteLine($"Getting weather for {location}");
        
        // Simulate API call delay
        await Task.Delay(100);
        
        // Simulate weather data
        var random = new Random();
        var temperature = random.Next(60, 85);
        var conditions = new[] { "sunny", "cloudy", "partly cloudy", "rainy" };
        var condition = conditions[random.Next(conditions.Length)];
        
        return $"The weather in {location} is {condition} with a temperature of {temperature}°F";
    }
    
    [KernelFunction, Description("Get weather forecast for a location")]
    public async Task<string> GetWeatherForecastAsync(string location, int days = 5)
    {
        Console.WriteLine($"Getting {days} day forecast for {location}");
        
        await Task.Delay(200);
        
        var forecast = $"Weather forecast for {location} for the next {days} days:\n";
        var random = new Random();
        
        for (int i = 1; i <= days; i++)
        {
            var temp = random.Next(65, 80);
            var conditions = new[] { "sunny", "cloudy", "partly cloudy", "rainy" };
            var condition = conditions[random.Next(conditions.Length)];
            forecast += $"Day {i}: {condition}, {temp}°F\n";
        }
        
        return forecast;
    }
}