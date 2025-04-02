#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0050
#pragma warning disable SKEXP0052
#pragma warning disable SKEXP0070

using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.Postgres;
using Microsoft.SemanticKernel.Plugins.Memory;
using Npgsql;
using System.Globalization;

var question = "Do you know what the exchange rate is for 1 Euro compared to 1 Iranian rial?";
Console.WriteLine($"❓ Our question is: {question}");
Console.WriteLine("1st: Ask directly from Llama3.2 (3 billion)");
Console.WriteLine("2nd: Ask via RAG technique with Llama3.2 (3 billion)");
Console.WriteLine("***************************************");

var builder = Kernel.CreateBuilder();

builder.AddOllamaChatCompletion(
    modelId: "llama3.2:3b",
    endpoint: new Uri("http://127.0.0.1:11434"));

builder.AddOllamaTextEmbeddingGeneration(
    modelId: "llama3.2:3b",
    endpoint: new Uri("http://127.0.0.1:11434")
);

var kernel = builder.Build();

Console.WriteLine($"Llama3.2's direct answer:");
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
Console.WriteLine($"Llama3.2's answer with RAG (using pgvector):");
Console.WriteLine("");

var embeddingGenerator = kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();

// Configure PostgreSQL connection
var connectionString = "Host=localhost;Port=5432;Database=ragdb;User Id=user;Password=pass;";
var schema = "public";

// Create pgvector memory store
var postgresMemoryStore = new PostgresMemoryStore(
    connectionString: connectionString,
    schema: schema,
    vectorSize: 3072
);


var memory = new SemanticTextMemory(postgresMemoryStore, embeddingGenerator);
var memoryPlugin = new TextMemoryPlugin(memory);
kernel.ImportPluginFromObject(memoryPlugin);
const string MemoryCollectionName = "exchangeRates";

string url = "https://bonbast.org";
var exchangeRates = await GetExchangeRatesAsync(url);

int counter = 0;
foreach (var rate in exchangeRates)
{
    string memoryKey = $"{rate.Key} to Iranian Rial";
    await memory.SaveInformationAsync(
        MemoryCollectionName,
        id: memoryKey,
        text: $"1 {rate.Key} equals {rate.Value * 10} Iranian Rials."
    );
    counter++;
}
Console.WriteLine($"{counter} rates added to the pgvector database...");


var settings = new OllamaPromptExecutionSettings();

var prompt = @"
Question: {{$input}}
Retrieved Data: {{Recall}}

Using the above retrieved data from our PostgreSQL database, please provide:
1. The current exchange rate for 1 Euro compared to 1 Iranian Rial
2. The source of the data
3. Any relevant contextual information about the rate";

var arguments = new KernelArguments(settings)
{
    { "input", question },
    { "collection", MemoryCollectionName },
    { "relevance", 0.8 }
};

string[] currencyPairs = { "Euro to Iranian Rial", "EUR to IRR", "Euro/IRR" };
foreach (var pair in currencyPairs)
{
    if (question.Contains(pair, StringComparison.OrdinalIgnoreCase))
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

Console.WriteLine("\nAll done!");

/*
// ========== Dummy Implementation of GetExchangeRatesAsync ==========
static async Task<Dictionary<string, double>> GetExchangeRatesAsync(string url)
{
    await Task.Delay(500); // Simulate delay

    return new Dictionary<string, double>
    {
        { "USD", 610000 },
        { "EUR", 665000 },
        { "GBP", 775000 }
    };
}
*/

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
