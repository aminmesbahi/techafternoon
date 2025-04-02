#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable 

using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOllamaTextEmbeddingGeneration(
            modelId: "mxbai-embed-large",// This will be our embedding model, something small with 335M parameters
            endpoint: new Uri("http://localhost:11434")
        );
        var kernel = builder.Build();

        // Get the embedding generation service from the kernel
        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // Define the input text to be embedded
        string inputText = "Hello, world!";

        // Generate the embedding for the input text
        var embedding = await embeddingService.GenerateEmbeddingAsync(inputText);
        var embeddingMemory = await embeddingService.GenerateEmbeddingAsync(inputText);
        var embeddingSpan = embeddingMemory.Span;
        // Output the embedding to the console
        Console.WriteLine($"Embedding for '{inputText}':");
        //Console.WriteLine($"Embedding Length: '{embeddingSpan.Length}'!");
        for (int i = 0; i < embeddingSpan.Length; i++)
        {
            Console.Write(embeddingSpan[i]);
            if (i < embeddingSpan.Length - 1)
            {
                Console.Write(", ");
            }
        }
        Console.WriteLine();
    }
}
