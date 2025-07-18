using System.ComponentModel;
using Microsoft.SemanticKernel;
using sementic_kernel_openai_integration.Models;
using SementicKernelOllama.Services;

namespace sementic_kernel_openai_integration.Plugins;

public class PDFStudyPlugin
{
    private readonly IStudyService _studyService;
    private StudySession? _currentSession;

    public PDFStudyPlugin(IStudyService studyService)
    {
        _studyService = studyService;
    }

    [KernelFunction, Description("Load a PDF document for studying")]
    public async Task<string> LoadPDFDocument(string pdfPath, string subject = "General")
    {
        try
        {
            _currentSession = await _studyService.CreateStudySessionAsync(pdfPath, subject);
            return $"✅ Loaded document: {_currentSession.DocumentName} for {subject} studies";
        }
        catch (Exception ex)
        {
            return $"❌ Error loading PDF: {ex.Message}";
        }
    }

    [KernelFunction, Description("Generate multiple choice questions from the loaded document")]
    public async Task<string> GenerateQuizQuestions(int numberOfQuestions = 5)
    {
        if (_currentSession == null)
        {
            return "❌ No document loaded. Please load a PDF first using LoadPDFDocument.";
        }

        try
        {
            var questions = await _studyService.GenerateMCQuestionsAsync(_currentSession, numberOfQuestions);
            var result = "📝 Generated Quiz Questions:\n\n";
            
            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                result += $"Question {i + 1}: {q.Question}\n";
                foreach (var option in q.Options)
                {
                    result += $"   {option}\n";
                }
                result += $"Correct Answer: {q.CorrectAnswer}\n";
                result += $"Explanation: {q.Explanation}\n\n";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"❌ Error generating questions: {ex.Message}";
        }
    }

    [KernelFunction, Description("Answer a question based on the loaded document")]
    public async Task<string> AnswerFromDocument(string question)
    {
        if (_currentSession == null)
        {
            return "❌ No document loaded. Please load a PDF first using LoadPDFDocument.";
        }

        try
        {
            var answer = await _studyService.AnswerQuestionFromDocumentAsync(_currentSession, question);
            
            if (!answer.IsFromDocument)
            {
                return "❌ This question cannot be answered from the loaded document.";
            }

            return $"""
            📖 Answer: {answer.Answer}
            
            📄 Source: "{answer.SourceQuote}"
            
            🎯 Confidence: {answer.Confidence}/10
            """;
        }
        catch (Exception ex)
        {
            return $"❌ Error answering question: {ex.Message}";
        }
    }

    [KernelFunction, Description("Get a summary of the loaded document")]
    public async Task<string> SummarizeDocument()
    {
        if (_currentSession == null)
        {
            return "❌ No document loaded. Please load a PDF first using LoadPDFDocument.";
        }

        try
        {
            var summary = await _studyService.SummarizeDocumentAsync(_currentSession);
            return $"📋 Document Summary:\n\n{summary}";
        }
        catch (Exception ex)
        {
            return $"❌ Error creating summary: {ex.Message}";
        }
    }

    [KernelFunction, Description("Get study topics from the loaded document")]
    public async Task<string> GetStudyTopics()
    {
        if (_currentSession == null)
        {
            return "❌ No document loaded. Please load a PDF first using LoadPDFDocument.";
        }

        try
        {
            var topics = await _studyService.GenerateStudyTopicsAsync(_currentSession);
            var result = "🎯 Key Study Topics:\n\n";
            
            for (int i = 0; i < topics.Count; i++)
            {
                result += $"{i + 1}. {topics[i]}\n";
            }

            return result;
        }
        catch (Exception ex)
        {
            return $"❌ Error extracting topics: {ex.Message}";
        }
    }

    [KernelFunction, Description("Explain a specific concept from the document")]
    public async Task<string> ExplainConcept(string concept)
    {
        if (_currentSession == null)
        {
            return "❌ No document loaded. Please load a PDF first using LoadPDFDocument.";
        }

        try
        {
            var explanation = await _studyService.ExplainConceptAsync(_currentSession, concept);
            return $"💡 Explanation of '{concept}':\n\n{explanation}";
        }
        catch (Exception ex)
        {
            return $"❌ Error explaining concept: {ex.Message}";
        }
    }
}