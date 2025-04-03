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
❓ Our question is: Do you know what the exchange rate is for 1 Euro compared to 1 Iranian rial?
1st: Ask directly from Llama3.2 (3 billion)
Llama3.2's direct answer:
> Sorry, I don't have access to real-time currency data.

***************************************
Press Enter to use RAG
***************************************
Llama3.2's answer with RAG (using Qdrant):
> Based on retrieved data, 1 Euro equals approximately 6,650,000 Iranian Rials. Source: bonbast.org. Last update: 2025/04/03 14:25:00.
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