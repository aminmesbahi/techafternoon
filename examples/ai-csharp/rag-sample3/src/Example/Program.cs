#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052

using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;

var question = "Do you know what the exchange rate is for 1 Euro compared to 1 Iranian rial?";
Console.WriteLine($"❓ Our question is: {question}");
Console.WriteLine("1st: Ask directly from llama3.2 (3 billion)");
Console.WriteLine("2nd: Ask via RAG technique with PDF input using llama3.2 (3 billion)");
Console.WriteLine("***************************************");

var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOllamaChatCompletion(
    modelId: "llama3.2:3b",
    endpoint: new Uri("http://127.0.0.1:11434"));

var kernel = kernelBuilder.Build();

// First response without RAG
Console.WriteLine($"llama3.2's initial answer:");
var response = kernel.InvokePromptStreamingAsync(question);
await foreach (var result in response) Console.Write(result);

Console.WriteLine("\n***************************************");
Console.WriteLine("Press Enter to use RAG with PDF");
Console.ReadLine();
Console.WriteLine("***************************************");
Console.WriteLine($"llama3.2's answer with RAG (using PDF content):\n");

// Configure Kernel Memory for PDF processing
var memoryBuilder = new KernelMemoryBuilder()
    .WithOllamaTextEmbeddingGeneration("llama3.2:3b", new Uri("http://127.0.0.1:11434"))
    .WithRedisMemoryDb("your-redis-connection-string"); // Replace with your Redis connection string

var memory = memoryBuilder.Build();

// Ingest PDF document (replace with your PDF file path)
await memory.ImportDocumentAsync(
    documentPath: "exchange_rates.pdf", // Your PDF file path
    documentId: "exchange_rates",
    steps: Constants.PipelineWithoutSummary);

// Create memory plugin
var memoryPlugin = new MemoryPlugin(memory);
kernel.ImportPluginFromObject(memoryPlugin, "pdf_memory");

// Create RAG prompt with PDF context
var prompt = """
    Question: {{$input}}
    
    PDF Context:
    {{pdf_memory.Ask ($input)}}
    
    Using information from the PDF document, provide:
    1. The current exchange rate for 1 Euro to Iranian Rial
    2. The date of the exchange rate information
    3. Any relevant details about the rate fluctuations
    """;

var settings = new OllamaPromptExecutionSettings 
{
    Temperature = 0.3,
    MaxTokens = 500
};

var arguments = new KernelArguments(settings)
{
    ["input"] = question
};

// Execute RAG-enhanced query
response = kernel.InvokePromptStreamingAsync(prompt, arguments);
await foreach (var result in response)
{
    Console.Write(result);
}

Console.WriteLine("\n***************************************");