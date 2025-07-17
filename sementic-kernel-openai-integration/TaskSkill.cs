using Microsoft.SemanticKernel;


namespace sementic_kernel_openai_integration;

public class TaskSkill
{
    /// <summary>
    /// Logs the given task string to the console.
    /// </summary>
    /// <param name="task">Task description</param>
    /// <returns>Task for async flow</returns>
    public Task LogTask(string task)
    {
        Console.WriteLine("📌 Task Logged: " + task);
        return Task.CompletedTask;
    }
}
