using System.Text.Json;
using sementic_kernel_openai_integration.Models;

namespace sementic_kernel_openai_integration.Services;

public class StudyService : IStudyService
{
    private readonly ISemanticKernelService _kernelService;
    private readonly IPDFService _pdfService;

    public StudyService(ISemanticKernelService kernelService, IPDFService pdfService)
    {
        _kernelService = kernelService;
        _pdfService = pdfService;
    }

    public async Task<StudySession> CreateStudySessionAsync(string pdfPath, string subject)
    {
        try
        {
            var pdfResult = await _pdfService.ExtractTextFromPDFAsync(pdfPath);
            
            if (!pdfResult.Success)
            {
                Console.WriteLine($"⚠️ PDF processing failed: {pdfResult.ErrorMessage}");
                
                // Fallback: Create session with sample content
                var fallbackSession = CreateFallbackSession(pdfPath, subject);
                Console.WriteLine("✅ Created fallback study session with sample content");
                return fallbackSession;
            }

            // Check if we actually got text content
            if (string.IsNullOrWhiteSpace(pdfResult.FullText) || !pdfResult.TextChunks.Any())
            {
                Console.WriteLine("⚠️ No text content extracted from PDF. Creating fallback session...");
                var fallbackSession = CreateFallbackSession(pdfPath, subject);
                return fallbackSession;
            }

            var session = new StudySession
            {
                DocumentName = Path.GetFileName(pdfPath),
                Subject = subject,
                DocumentChunks = pdfResult.TextChunks
            };

            Console.WriteLine($"✅ Study session created for: {session.DocumentName}");
            Console.WriteLine($"📄 Pages: {pdfResult.PageCount}, Chunks: {pdfResult.TextChunks.Count}");
            
            return session;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error creating study session: {ex.Message}");
            
            // Ultimate fallback
            var fallbackSession = CreateFallbackSession(pdfPath, subject);
            Console.WriteLine("✅ Created emergency fallback session");
            return fallbackSession;
        }
    }

    private StudySession CreateFallbackSession(string pdfPath, string subject)
    {
        var fallbackContent = $"""
        Fallback Study Content for: {subject}

        Chapter 1: Introduction
        This is a sample study session created because the original PDF could not be processed. 
        This content demonstrates how the study assistant works with text-based learning materials.

        Key Concepts:
        - Understanding the fundamentals is crucial for advanced learning
        - Practice and repetition help reinforce important concepts  
        - Active learning through questions improves retention
        - Breaking down complex topics into smaller parts aids comprehension

        Chapter 2: Core Principles
        When studying any subject, it's important to:
        1. Identify the main topics and themes
        2. Create connections between related concepts
        3. Practice applying knowledge through examples
        4. Test understanding through self-assessment

        Chapter 3: Study Strategies
        Effective study methods include:
        - Summarizing key points in your own words
        - Creating visual aids like diagrams or mind maps
        - Teaching concepts to others to test understanding
        - Regular review and spaced repetition

        This sample content allows you to test all the study assistant features:
        - Question generation
        - Concept explanations  
        - Summary creation
        - Topic extraction
        """;

        return new StudySession
        {
            DocumentName = Path.GetFileName(pdfPath) + " (Fallback)",
            Subject = subject,
            DocumentChunks = new List<string> { fallbackContent }
        };
    }

    public async Task<List<MCQQuestion>> GenerateMCQuestionsAsync(StudySession session, int numberOfQuestions = 5)
    {
        var questions = new List<MCQQuestion>();
        
        // Fallback: Check if we have document chunks
        if (!session.DocumentChunks.Any())
        {
            Console.WriteLine("⚠️ No document chunks available. Creating sample questions...");
            return CreateSampleQuestions(numberOfQuestions);
        }

        var chunksPerQuestion = Math.Max(1, session.DocumentChunks.Count / numberOfQuestions);

        for (int i = 0; i < numberOfQuestions && i * chunksPerQuestion < session.DocumentChunks.Count; i++)
        {
            var chunkIndex = i * chunksPerQuestion;
            var chunk = session.DocumentChunks[chunkIndex];
            
            // Skip empty chunks
            if (string.IsNullOrWhiteSpace(chunk))
            {
                Console.WriteLine($"⚠️ Skipping empty chunk {chunkIndex}");
                continue;
            }

            var prompt = @"Create 1 multiple choice question ONLY from this content. Do not use external knowledge.

STRICT RULES:
- Question must be answerable from the provided text
- All options must be plausible based on the context
- Include one clearly correct answer
- Provide a brief explanation

Content: " + chunk + @"

Format your response as JSON:
{
    ""question"": ""Your question here"",
    ""options"": [""A) option1"", ""B) option2"", ""C) option3"", ""D) option4""],
    ""correctAnswer"": ""A"",
    ""explanation"": ""Brief explanation why this is correct"",
    ""sourceText"": ""Relevant quote from the content""
}";

            try
            {
                var response = await _kernelService.InvokePromptAsync(prompt);
                var mcq = ParseMCQFromResponse(response, chunk);
                if (mcq != null)
                {
                    questions.Add(mcq);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating question {i + 1}: {ex.Message}");
                
                // Fallback: Create a basic question from the chunk
                var fallbackQuestion = CreateFallbackQuestion(chunk);
                questions.Add(fallbackQuestion);
            }
        }

        // Final fallback: If no questions generated, create samples
        if (!questions.Any())
        {
            Console.WriteLine("⚠️ No questions could be generated. Creating sample questions...");
            questions = CreateSampleQuestions(numberOfQuestions);
        }

        session.GeneratedQuestions = questions;
        return questions;
    }

    private List<MCQQuestion> CreateSampleQuestions(int numberOfQuestions)
    {
        var sampleQuestions = new List<MCQQuestion>();
        
        for (int i = 0; i < numberOfQuestions; i++)
        {
            sampleQuestions.Add(new MCQQuestion
            {
                Question = $"Sample Question {i + 1}: What is a key concept in academic study?",
                Options = new List<string>
                {
                    "A) Taking detailed notes",
                    "B) Ignoring the material", 
                    "C) Memorizing without understanding",
                    "D) Studying only before exams"
                },
                CorrectAnswer = "A",
                Explanation = "Taking detailed notes helps in better understanding and retention of material.",
                SourceText = "Sample content for demonstration purposes"
            });
        }
        
        return sampleQuestions;
    }

    public async Task<StudyAnswer> AnswerQuestionFromDocumentAsync(StudySession session, string question)
    {
        try
        {
            // Fallback: Check if we have document chunks
            if (!session.DocumentChunks.Any())
            {
                return new StudyAnswer
                {
                    Answer = "No document content available to answer this question.",
                    SourceQuote = "",
                    Confidence = 0,
                    IsFromDocument = false
                };
            }

            // Find relevant chunks
            var relevantChunks = await _pdfService.FindRelevantChunksAsync(session.DocumentChunks, question, 3);
            
            // Fallback: If no relevant chunks found, use first few chunks
            if (!relevantChunks.Any())
            {
                relevantChunks = session.DocumentChunks.Take(2).ToList();
            }

            var combinedContext = string.Join("\n\n", relevantChunks);

            var prompt = @"STRICT INSTRUCTIONS: Answer ONLY based on the provided document content.
If the answer is not clearly stated in the content, respond with ""This information is not available in the provided document.""

Document Content:
" + combinedContext + @"

Question: " + question + @"

Provide your response in this format:
Answer: [Your answer based only on the document]
Source Quote: ""[Exact text from document that supports your answer]""
Confidence: [1-10 scale how confident you are this answer comes from the document]";

            var response = await _kernelService.InvokePromptAsync(prompt);
            return ParseStudyAnswer(response, relevantChunks);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error answering question: {ex.Message}");
            
            // Final fallback
            return new StudyAnswer
            {
                Answer = "Unable to process the question due to a technical error. Please try rephrasing your question.",
                SourceQuote = "",
                Confidence = 0,
                IsFromDocument = false
            };
        }
    }

    public async Task<string> SummarizeDocumentAsync(StudySession session, int maxLength = 500)
    {
        try
        {
            var allText = string.Join(" ", session.DocumentChunks);
            var truncatedText = allText.Length > 3000 ? allText.Substring(0, 3000) + "..." : allText;

            var prompt = @"Create a concise summary of this document content. 
Focus on the main concepts, key points, and important information.
Keep the summary under " + maxLength + @" words.

Document: " + truncatedText + @"

Summary:";

            return await _kernelService.InvokePromptAsync(prompt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error summarizing document: {ex.Message}");
            return "Unable to create summary. The document may contain complex content that requires manual review.";
        }
    }

    public async Task<List<string>> GenerateStudyTopicsAsync(StudySession session)
    {
        try
        {
            var allText = string.Join(" ", session.DocumentChunks.Take(3)); // Use first few chunks
            
            var prompt = @"Extract the main study topics from this content.
List 5-10 key topics that a student should focus on.

Content: " + allText + @"

Format as a simple list:
- Topic 1
- Topic 2
etc.";

            var response = await _kernelService.InvokePromptAsync(prompt);
            return ParseTopicsFromResponse(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating topics: {ex.Message}");
            return new List<string> { "General Study Concepts", "Key Principles", "Important Definitions", "Practical Applications" };
        }
    }

    public async Task<string> ExplainConceptAsync(StudySession session, string concept)
    {
        try
        {
            var relevantChunks = await _pdfService.FindRelevantChunksAsync(session.DocumentChunks, concept, 2);
            var combinedContext = string.Join("\n\n", relevantChunks);

            var prompt = @"Explain the concept of """ + concept + @""" based ONLY on the provided document content.
Make the explanation clear and easy to understand.
If the concept is not covered in the document, say so clearly.

Document Content:
" + combinedContext + @"

Explain """ + concept + @""":";

            return await _kernelService.InvokePromptAsync(prompt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error explaining concept: {ex.Message}");
            return $"Unable to explain the concept '{concept}' due to a technical error. Please try asking about a different concept.";
        }
    }

    private MCQQuestion? ParseMCQFromResponse(string response, string sourceChunk)
    {
        try
        {
            // Try to extract JSON from response
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonString = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var questionData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);
                
                if (questionData != null)
                {
                    return new MCQQuestion
                    {
                        Question = questionData.GetValueOrDefault("question", "").ToString() ?? "",
                        Options = JsonSerializer.Deserialize<List<string>>(questionData.GetValueOrDefault("options", "[]").ToString() ?? "[]") ?? new(),
                        CorrectAnswer = questionData.GetValueOrDefault("correctAnswer", "").ToString() ?? "",
                        Explanation = questionData.GetValueOrDefault("explanation", "").ToString() ?? "",
                        SourceText = questionData.GetValueOrDefault("sourceText", "").ToString() ?? ""
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JSON parsing failed: {ex.Message}");
        }

        // Fallback 1: Try to parse unstructured response
        try
        {
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var question = "";
            var options = new List<string>();
            var correctAnswer = "";
            var explanation = "";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.Contains("Question:") || trimmed.EndsWith("?"))
                {
                    question = trimmed.Replace("Question:", "").Trim();
                }
                else if (trimmed.StartsWith("A)") || trimmed.StartsWith("B)") || 
                         trimmed.StartsWith("C)") || trimmed.StartsWith("D)"))
                {
                    options.Add(trimmed);
                }
                else if (trimmed.Contains("Correct:") || trimmed.Contains("Answer:"))
                {
                    correctAnswer = ExtractAnswer(trimmed);
                }
                else if (trimmed.Contains("Explanation:"))
                {
                    explanation = trimmed.Replace("Explanation:", "").Trim();
                }
            }

            if (!string.IsNullOrEmpty(question) && options.Count >= 4)
            {
                return new MCQQuestion
                {
                    Question = question,
                    Options = options,
                    CorrectAnswer = correctAnswer,
                    Explanation = explanation,
                    SourceText = sourceChunk.Substring(0, Math.Min(100, sourceChunk.Length)) + "..."
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unstructured parsing failed: {ex.Message}");
        }

        // Fallback 2: Generate a basic question from the chunk
        Console.WriteLine("Using fallback question generation...");
        return CreateFallbackQuestion(sourceChunk);
    }

    private MCQQuestion CreateFallbackQuestion(string sourceChunk)
    {
        // Create a simple question from the source chunk
        var sentences = sourceChunk.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var firstSentence = sentences.FirstOrDefault()?.Trim() ?? "Content from document";
        
        // Extract key terms for options
        var words = firstSentence.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                 .Where(w => w.Length > 4)
                                 .Take(4)
                                 .ToList();

        while (words.Count < 4)
        {
            words.Add($"Option {words.Count + 1}");
        }

        return new MCQQuestion
        {
            Question = $"What is mentioned in this section of the document?",
            Options = new List<string>
            {
                $"A) {words[0]}",
                $"B) {words[1]}", 
                $"C) {words[2]}",
                $"D) {words[3]}"
            },
            CorrectAnswer = "A",
            Explanation = "Based on the content provided in the document",
            SourceText = firstSentence
        };
    }

    private string ExtractAnswer(string line)
    {
        var upperLine = line.ToUpper();
        if (upperLine.Contains("A")) return "A";
        if (upperLine.Contains("B")) return "B";
        if (upperLine.Contains("C")) return "C";
        if (upperLine.Contains("D")) return "D";
        return "A"; // Default fallback
    }

    private StudyAnswer ParseStudyAnswer(string response, List<string> sourceChunks)
    {
        var answer = new StudyAnswer();
        
        try
        {
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("Answer:", StringComparison.OrdinalIgnoreCase))
                {
                    answer.Answer = line.Substring(7).Trim();
                }
                else if (line.StartsWith("Source Quote:", StringComparison.OrdinalIgnoreCase))
                {
                    answer.SourceQuote = line.Substring(13).Trim().Trim('"');
                }
                else if (line.StartsWith("Confidence:", StringComparison.OrdinalIgnoreCase))
                {
                    if (double.TryParse(line.Substring(11).Trim(), out var confidence))
                    {
                        answer.Confidence = confidence;
                    }
                }
            }

            // Fallback parsing if structured format not found
            if (string.IsNullOrEmpty(answer.Answer))
            {
                // Use the entire response as the answer
                answer.Answer = response.Trim();
                answer.SourceQuote = sourceChunks.FirstOrDefault()?.Substring(0, Math.Min(100, sourceChunks.FirstOrDefault()?.Length ?? 0)) ?? "";
                answer.Confidence = 5; // Medium confidence for fallback
            }

            answer.IsFromDocument = !answer.Answer.Contains("not available in the provided document");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing study answer: {ex.Message}");
            
            // Ultimate fallback
            answer.Answer = "Unable to parse the response. Please try asking the question differently.";
            answer.SourceQuote = "";
            answer.Confidence = 0;
            answer.IsFromDocument = false;
        }

        return answer;
    }

    private List<string> ParseTopicsFromResponse(string response)
    {
        var topics = new List<string>();
        
        try
        {
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("-") || trimmed.StartsWith("•"))
                {
                    topics.Add(trimmed.Substring(1).Trim());
                }
                else if (Char.IsDigit(trimmed[0]) && trimmed.Contains('.'))
                {
                    var dotIndex = trimmed.IndexOf('.');
                    if (dotIndex > 0 && dotIndex < trimmed.Length - 1)
                    {
                        topics.Add(trimmed.Substring(dotIndex + 1).Trim());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing topics: {ex.Message}");
        }

        // Fallback if no topics parsed
        if (!topics.Any())
        {
            topics.AddRange(new[] { "Main Concepts", "Key Principles", "Important Definitions", "Study Points" });
        }

        return topics;
    }
}