using System.Text;
using System.Text.RegularExpressions;
using sementic_kernel_openai_integration.Models;
using UglyToad.PdfPig;

namespace SementicKernelOllama.Services;

public class PDFService : IPDFService
{
    public async Task<PDFProcessingResult> ExtractTextFromPDFAsync(string pdfPath)
    {
        var result = new PDFProcessingResult();
        try
        {
            if (!File.Exists(pdfPath))
            {
                result.ErrorMessage = "PDF file not found";
                return result;
            }

            var pdfBytes = await File.ReadAllBytesAsync(pdfPath);
            using var document = PdfDocument.Open(pdfBytes);

            
            var text = new StringBuilder();
            result.PageCount = document.NumberOfPages;

            for (int i = 1; i <= document.NumberOfPages; i++)
            {
                try
                {
                    var page = document.GetPage(i);
                    text.AppendLine($"\n--- Page {i} ---\n");

                    // Try to get text, fallback to words if needed
                    try
                    {
                        var pageText = page.Text;
                        text.AppendLine(pageText);
                    }
                    catch
                    {
                        // Fallback: extract words manually
                        var words = page.GetWords();
                        var pageContent = string.Join(" ", words.Select(w => w.Text));
                        text.AppendLine(pageContent);
                    }
                }
                catch (Exception pageEx)
                {
                    text.AppendLine($"Error reading page {i}: {pageEx.Message}");
                }
            }

            result.FullText = CleanText(text.ToString());
            result.TextChunks = ChunkText(result.FullText);
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error processing PDF: {ex.Message}";
        }

        return await Task.FromResult(result);    
    }

    public List<string> ChunkText(string text, int maxChunkSize = 2000)
    {
        var chunks = new List<string>();
        
        if (string.IsNullOrWhiteSpace(text))
        {
            return chunks;
        }

        // Simple sentence splitting
        var sentences = text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new StringBuilder();
        
        foreach (var sentence in sentences)
        {
            var cleanSentence = sentence.Trim();
            if (string.IsNullOrEmpty(cleanSentence)) continue;
            
            if (currentChunk.Length + cleanSentence.Length > maxChunkSize && currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
            }
            
            currentChunk.Append(cleanSentence + ". ");
        }
        
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString().Trim());
        }
        
        // Fallback if no sentences found
        if (chunks.Count == 0)
        {
            chunks.Add(text.Substring(0, Math.Min(text.Length, maxChunkSize)));
        }
        
        return chunks;
        
    }

    public async Task<List<string>> FindRelevantChunksAsync(List<string> chunks, string query, int maxChunks = 3)
    {
        if (!chunks.Any() || string.IsNullOrWhiteSpace(query))
        {
            return chunks.Take(maxChunks).ToList();
        }

        var relevantChunks = chunks
            .Select(chunk => new { 
                Chunk = chunk, 
                Score = CalculateRelevanceScore(chunk, query) 
            })
            .OrderByDescending(x => x.Score)
            .Take(maxChunks)
            .Select(x => x.Chunk)
            .ToList();

        return await Task.FromResult(relevantChunks);
        
    }
    
    private string CleanText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        text = Regex.Replace(text, @"\s+", " ");
        text = Regex.Replace(text, @"[\r\n]+", "\n");
        return text.Trim();
    }

    private int CalculateRelevanceScore(string text, string query)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
        {
            return 0;
        }

        var queryWords = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var textLower = text.ToLower();
        
        return queryWords.Count(word => textLower.Contains(word));
    }
}