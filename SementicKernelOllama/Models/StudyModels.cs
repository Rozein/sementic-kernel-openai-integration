namespace sementic_kernel_openai_integration.Models;

public class StudyAnswer
{
    public string Answer { get; set; } = string.Empty;
    public string SourceQuote { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public double Confidence { get; set; }
    public bool IsFromDocument { get; set; }
}

public class MCQQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string SourceText { get; set; } = string.Empty;
}

public class StudySession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<string> DocumentChunks { get; set; } = new();
    public List<MCQQuestion> GeneratedQuestions { get; set; } = new();
    public int CurrentQuestionIndex { get; set; } = 0;
    public int CorrectAnswers { get; set; } = 0;
}

public class PDFProcessingResult
{
    public bool Success { get; set; }
    public string FullText { get; set; } = string.Empty;
    public List<string> TextChunks { get; set; } = new();
    public int PageCount { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}