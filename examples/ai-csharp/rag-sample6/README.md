Here’s the completed `README.md` based on your code and setup:

---

# Example 6: RAG using Qdrant & Semantic Kernel

This example demonstrates how to use **RAG** (Retrieval-Augmented Generation) with **Qdrant** as the vector database and **Microsoft Semantic Kernel** with an **Ollama-hosted LLaMA 3.2 (3B)** model.

We use Semantic Kernel's memory plugins to enrich the LLM's responses with real-time exchange rates scraped from [bonbast.org](https://bonbast.org), stored in Qdrant, and retrieved based on the user’s query.

---

## 🔧 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- [Docker](https://www.docker.com/)
- [Ollama](https://ollama.com/) running `llama3.2:3b`
- Internet access to query real-time exchange rates
- NuGet packages:
  - `Microsoft.SemanticKernel`
  - `Microsoft.SemanticKernel.Connectors.Qdrant`
  - `Microsoft.SemanticKernel.Connectors.Ollama`
  - `Microsoft.Extensions.DependencyInjection`
  - `HtmlAgilityPack`

---

## 🧠 What is Qdrant?

**Qdrant** is a high-performance, open-source vector database optimized for storing and querying embeddings. It supports advanced filtering and metadata storage, making it ideal for building semantic search, RAG pipelines, and recommendation engines.

In this example, Qdrant is used to store embedded currency rate information, and Semantic Kernel queries this vector store to retrieve relevant context before passing it to the LLM.

> Learn more: [https://qdrant.tech](https://qdrant.tech)

---

## 🐳 Running Qdrant with Docker

To run a local Qdrant instance, use the provided `docker-compose` setup:

```bash
docker compose -f .\compose_qdrant.yaml up -d
```

**compose_qdrant.yaml**
```yaml
services:
  qdrant:
    image: qdrant/qdrant:latest
    container_name: qdrant
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant_storage:/qdrant/storage
    environment:
      QDRANT__SERVICE__API_KEY: "my-secret-key" 
      QDRANT__STORAGE__SNAPSHOT_PERIOD_SEC: 3600
    restart: unless-stopped

volumes:
  qdrant_storage:
```

---

## 🚀 Running the Example

1. **Ensure Qdrant is running**:
   ```bash
   docker compose -f .\compose_qdrant.yaml up -d
   ```

2. **Start Ollama with LLaMA 3.2 (3B)**:
   ```bash
   ollama run llama3.2:3b
   ```

3. **Run the C# application**:
   ```bash
   dotnet run
   ```

---

## 💡 What this example does

### 🔹 Step 1: Ask LLaMA 3.2 directly
We ask the LLM a question:
> _"Do you know what the exchange rate is for 1 Euro compared to 1 Iranian rial?"_

This tests the LLM's answer without any external context (zero-shot).

---

### 🔹 Step 2: Use RAG with Qdrant
1. Scrapes the latest currency rates from **bonbast.org**
2. Stores those in Qdrant as vector-embedded memory items
3. Uses Semantic Kernel to retrieve relevant entries from Qdrant
4. Injects the results into a custom prompt
5. LLaMA generates a better-informed response using real-time data

---

## 📎 Notes

- You can customize the scraping logic to pull more or different data.
- Update the embedding model, prompt template, or LLM provider as needed.
- This example uses a dummy fallback for `GetExchangeRatesAsync()` if the site is not reachable (see comments in the code).

---

## 📂 Structure

```
.
├── compose_qdrant.yaml         # Qdrant docker config
├── Program.cs                  # Main example with Semantic Kernel and Qdrant
└── README.md                   # You're here!
```

---

## 📬 Sample Output

```bash
dotnet run --project .\Example\Example.csproj
❓ Our question is: Do you know what the exchange rate is for 1 EUR compared to 1 IRR?
1) Ask directly from Llama3.2 (3b)
2) Use RAG with Qdrant to retrieve updated info, then ask Llama3.2
***************************************
--- Llama3.2's direct answer ---
I can't provide real-time or the most current exchange rates, but I can give you an approximate conversion rate.

As of my knowledge cutoff in 2023, 1 EUR was approximately equal to 17,000 - 18,000 IRR (Iranian Rial). However, please note that exchange rates can fluctuate frequently and may have changed since my knowledge cutoff. For the most up-to-date and accurate conversion rate, I recommend checking a reliable currency exchange website or service, such as XE.com or Oanda.com.
***************************************
Press Enter to use RAG (Qdrant)...

***************************************
Llama3.2's answer with RAG (using Qdrant):

5 rates added to Qdrant DB.
**Question:**
Do you know what the exchange rate is for 1 EUR compared to 1 IRR?

**Retrieved Exchange Rate Snippet:**


1 USD = 1,036,920 IRR
Last update: 4/4/2025 3:55:05 PM
1 GBP = 1,342,120 IRR
Last update: 4/4/2025 3:55:05 PM
1 EUR = 1,121,180 IRR
Last update: 4/4/2025 3:55:05 PM

**Answer:**

- Exchange Rate:
1 EUR = 1,121,180 IRR

- Last Update:
4/4/2025 3:55:05 PM

- Notes:
The exchange rate for 1 EUR to 1 IRR is 1,121,180. The last update date is 4/4/2025 3:55:05 PM.
All done!
```



###

### Docker compose for Qdrant 
First, we need a Qdrant as the vector database

```bash
docker compose -f .\compose_qdrant.yaml up -d
```

```
services:
  qdrant:
    image: qdrant/qdrant:latest
    container_name: qdrant
    ports:
      - "6333:6333"
      - "6334:6334"
    volumes:
      - qdrant_storage:/qdrant/storage
    environment:
      QDRANT__SERVICE__API_KEY: "my-secret-key" 
      QDRANT__STORAGE__SNAPSHOT_PERIOD_SEC: 3600
    restart: unless-stopped

volumes:
  qdrant_storage:
```