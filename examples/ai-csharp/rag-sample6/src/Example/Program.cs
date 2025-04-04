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
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Memory;
using System.Globalization;

public class Program
{
    public static async Task Main()
    {
        var question = "Do you know what the exchange rate is for 1 EUR compared to 1 IRR?";
        Console.WriteLine($"❓ Our question is: {question}");

        Console.WriteLine("1) Ask directly from Llama3.2 (3b)");
        Console.WriteLine("2) Use RAG with Qdrant to retrieve updated info, then ask Llama3.2");
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

        Console.WriteLine($"--- Llama3.2's direct answer ---");
        var directResponse = kernel.InvokePromptStreamingAsync(question);
        await foreach (var part in directResponse)
        {
            Console.Write(part);
        }

        Console.WriteLine("\n***************************************");
        Console.WriteLine("Press Enter to use RAG (Qdrant)...");
        Console.ReadLine();
        Console.WriteLine("***************************************");
        Console.WriteLine("Llama3.2's answer with RAG (using Qdrant):\n");

        var embeddingGenerator = kernel.Services.GetService(typeof(ITextEmbeddingGenerationService))
                                 as ITextEmbeddingGenerationService;
        if (embeddingGenerator == null)
        {
            throw new InvalidOperationException("No embedding service found in the kernel!");
        }

        var apiKey = "my-secret-key";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

        var qdrantEndpoint = "http://localhost:6333";
        var qdrantMemoryStore = new QdrantMemoryStore(
            endpoint: new Uri(qdrantEndpoint).ToString(),
            vectorSize: 3072,
            httpClient: httpClient
        );

        var memory = new SemanticTextMemory(qdrantMemoryStore, embeddingGenerator);
        var memoryPlugin = new TextMemoryPlugin(memory);
        kernel.ImportPluginFromObject(memoryPlugin);

        const string MemoryCollectionName = "exchangeRates";
        string url = "https://bonbast.org";
        var exchangeRates = await GetExchangeRatesAsync(url);

        int counter = 0;
        foreach (var rate in exchangeRates)
        {
            string memoryKey = $"{rate.Key} to IRR";
            string snippet = $@"
1 {rate.Key} = {rate.Value.AverageRate * 10:N0} IRR
Last update: {rate.Value.LastUpdate}
";

            await memory.SaveInformationAsync(
                collection: MemoryCollectionName,
                id: memoryKey,
                text: snippet
            );
            counter++;
        }
        Console.WriteLine($"{counter} rates added to Qdrant DB.");

        var searchResults = memory.SearchAsync(
    "exchangeRates",
    "EUR to IRR",
    limit: 3,
    minRelevanceScore: 0.0f
);

        List<MemoryQueryResult> resultsList = new();
        await foreach (var item in searchResults)
        {
            resultsList.Add(item);
        }

        var retrievedText = (resultsList.Count == 0)
            ? "(No data found)"
            : string.Join("\n", resultsList.Select(r => r.Metadata.Text));

        //Console.WriteLine("\nDEBUG: snippet from Qdrant memory:\n" + retrievedText + "\n");

        var finalPrompt = $@"
You are a financial assistant.

**Question:**
{question}

**Retrieved Exchange Rate Snippet:**
{retrievedText}

Please:
1. Extract the exact exchange rate for EUR (EUR) to IRR (IRR).
2. State the last update date from the snippet above.
3. If no snippet is available or it doesn't mention EUR, say 'Data not available.'

Answer format:
- Exchange Rate:
- Last Update:
- Notes:
";

        var ragResponse = kernel.InvokePromptStreamingAsync(finalPrompt);
        await foreach (var part2 in ragResponse)
        {
            Console.Write(part2);
        }

        Console.WriteLine("\nAll done!");
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

                        exchangeRates[currency] = (averageRate, DateTime.Now.ToString());

                        //Console.WriteLine($"Currency: {currency}, Avg Rate: {averageRate}, "
                        //    + $"Last Update: {DateTime.Now}");
                    }
                }
            }
        }
        return exchangeRates;
    }
}
