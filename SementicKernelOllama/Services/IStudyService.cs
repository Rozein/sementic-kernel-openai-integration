using sementic_kernel_openai_integration.Models;

namespace SementicKernelOllama.Services;

public interface IStudyService
{
    Task<StudySession> CreateStudySessionAsync(string pdfPath, string subject);
    Task<List<MCQQuestion>> GenerateMCQuestionsAsync(StudySession session, int numberOfQuestions = 5);
    Task<StudyAnswer> AnswerQuestionFromDocumentAsync(StudySession session, string question);
    Task<string> SummarizeDocumentAsync(StudySession session, int maxLength = 500);
    Task<List<string>> GenerateStudyTopicsAsync(StudySession session);
    Task<string> ExplainConceptAsync(StudySession session, string concept);
}