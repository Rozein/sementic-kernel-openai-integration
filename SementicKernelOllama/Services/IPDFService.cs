using sementic_kernel_openai_integration.Models;

namespace SementicKernelOllama.Services;

public interface IPDFService
{
    Task<PDFProcessingResult> ExtractTextFromPDFAsync(string pdfPath);
    List<string> ChunkText(string text, int maxChunkSize = 2000);
    Task<List<string>> FindRelevantChunksAsync(List<string> chunks, string query, int maxChunks = 3);
}