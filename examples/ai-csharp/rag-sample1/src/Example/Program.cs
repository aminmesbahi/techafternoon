#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable 

using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using System.Globalization;

var question = "Do you know what the exchange rate is for 1 Euro compared to 1 Iranian rial?";
Console.WriteLine($"❓ our question is: {question}");
Console.WriteLine("1st: ask directly from llama3.2 (3 billion)");
Console.WriteLine("2nd: ask via RAG technique with llama3.2 (3 billion)");
Console.WriteLine("***************************************");

var builder = Kernel.CreateBuilder();
builder.AddOllamaChatCompletion(
    modelId: "llama3.2:3b",
    endpoint: new Uri("http://127.0.0.1:11434"));

builder.AddOllamaTextEmbeddingGeneration(modelId: "llama3.2:3b",
    endpoint: new Uri("http://127.0.0.1:11434")
    );

Kernel kernel = builder.Build();

Console.WriteLine($"llama3.2's answer:");
var response = kernel.InvokePromptStreamingAsync(question);
await foreach (var result in response)
{
    Console.Write(result);
}

Console.WriteLine("");
Console.WriteLine("***************************************");
Console.WriteLine("Press Enter to use RAG");
Console.ReadLine();
Console.WriteLine("***************************************");
Console.WriteLine($"llama3.2's answer with RAG (using semantic memory):");
Console.WriteLine("");

var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
var memory = new SemanticTextMemory(new VolatileMemoryStore(), embeddingGenerator);

const string MemoryCollectionName = "exchangeRates";

string url = "https://bonbast.org";
var exchangeRates = await GetExchangeRatesAsync(url);

int counter = 0;
foreach (var rate in exchangeRates)
{
    string memoryKey = $"{rate.Key} to Iranian Rial";
    await memory.SaveInformationAsync(MemoryCollectionName, id: memoryKey, text: $"1 {rate.Key} equals {rate.Value.AverageRate * 10} Iranian Rials...");
    counter += 1;
}
Console.WriteLine($"{counter} rates added to the memory... wait please");
TextMemoryPlugin memoryPlugin = new(memory);

kernel.ImportPluginFromObject(memoryPlugin);

OllamaPromptExecutionSettings settings = new OllamaPromptExecutionSettings();

var prompt = @"
    Question: {{$input}}
    Retrieved Data: {{Recall}}

    Using the above retrieved data, please provide the current exchange rate for 1 Euro compared to 1 Iranian Rial.";

var arguments = new KernelArguments(settings)
{
    { "input", question },
    { "collection", MemoryCollectionName }
};

string[] currencyPairs = { "Euro to Iranian Rial", "EUR to IRR", "Euro/IRR" };

foreach (var pair in currencyPairs)
{
    if (question.Contains(pair))
    {
        arguments.Add("currencyPair", pair);
        break;
    }
}

response = kernel.InvokePromptStreamingAsync(prompt, arguments);
await foreach (var result in response)
{
    Console.Write(result);
}


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
