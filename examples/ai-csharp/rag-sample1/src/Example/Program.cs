#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052

using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;

var question = "with what price can I buy 1 Euro using Rial?";
Console.WriteLine($"I will try to answer the question: {question}");
Console.WriteLine("First, with asking the question directly to the Phi-3 model via Ollama");
Console.WriteLine("Second, get actual data from bonbast, and then use semantic memory and ask the question again");
Console.WriteLine("");

// Create a chat completion service
var builder = Kernel.CreateBuilder();
builder.AddOpenAIChatCompletion(
    modelId: "phi3.5",
    endpoint: new Uri("http://localhost:11434"),
    apiKey: "apikey");
builder.AddLocalTextEmbeddingGeneration();
Kernel kernel = builder.Build();

Console.WriteLine($"Phi-3 response (no memory).");
var response = kernel.InvokePromptStreamingAsync(question);
await foreach (var result in response)
{
    Console.Write(result);
}

// separator
Console.WriteLine("");
Console.WriteLine("==============");
Console.WriteLine("Press Enter to continue");
Console.ReadLine();
Console.WriteLine("==============");
Console.WriteLine($"Phi-3 response (using semantic memory).");
Console.WriteLine("");

var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);


string url = "https://bonbast.org";
var exchangeRates = await GetExchangeRatesAsync(url);

const string MemoryCollectionName = "exchangeRates";
int counter =0;
foreach (var rate in exchangeRates)
{
    await memory.SaveInformationAsync(MemoryCollectionName, id: $"rate{counter}", text: $"Currency: {rate.Key}, Average Rate: {rate.Value.AverageRate}, Last Update: {rate.Value.LastUpdate}");
}

TextMemoryPlugin memoryPlugin = new(memory);

kernel.ImportPluginFromObject(memoryPlugin);

OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};


var prompt = @"
    Question: {{$input}}
    Answer the question using the memory content: {{Recall}}";

var arguments = new KernelArguments(settings)
{
    { "input", question },
    { "collection", MemoryCollectionName }
};



response = kernel.InvokePromptStreamingAsync(prompt, arguments);
await foreach (var result in response)
{
    Console.Write(result);
}

Console.WriteLine($"");



static async Task<Dictionary<string, (double AverageRate, string LastUpdate)>> GetExchangeRatesAsync(string url)
{
    var exchangeRates = new Dictionary<string, (double AverageRate, string LastUpdate)>();

    using (HttpClient client = new HttpClient())
    {
        var response = await client.GetStringAsync(url);
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        var dateNode = htmlDoc.DocumentNode.SelectSingleNode("//span[@id='shamsi-date']");
        string lastUpdate = dateNode?.InnerText.Trim() ?? "Unknown";

        var rows = htmlDoc.DocumentNode.SelectNodes("//table[@id='exchange-table']//tr[position()>1]");
        if (rows != null)
        {
            foreach (var row in rows)
            {
                var currencyNode = row.SelectSingleNode(".//td[1]");
                var buyRateNode = row.SelectSingleNode(".//td[2]");
                var sellRateNode = row.SelectSingleNode(".//td[3]");

                if (currencyNode != null && buyRateNode != null && sellRateNode != null)
                {
                    string currency = currencyNode.InnerText.Trim();
                    double buyRate = Convert.ToDouble(buyRateNode.InnerText.Trim().Replace(",", ""));
                    double sellRate = Convert.ToDouble(sellRateNode.InnerText.Trim().Replace(",", ""));
                    double averageRate = (buyRate + sellRate) / 2;

                    exchangeRates[currency] = (averageRate, lastUpdate);
                }
            }
        }
    }
    return exchangeRates;
}