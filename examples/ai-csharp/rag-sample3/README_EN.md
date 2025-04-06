# Example: Using Semantic Kernel for RAG with PDF Context

This example demonstrates how to use **Semantic Kernel** to implement a Retrieval-Augmented Generation (RAG) pipeline with PDF document context. The pipeline integrates **Ollama-hosted LLaMA 3.2 (3B)** for chat completion and **Redis** for memory storage.

---

## 🔧 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- [Redis](https://redis.io/) for memory storage
- [Ollama](https://ollama.com/) running `llama3.2:3b`
- A PDF document containing exchange rate information
- NuGet packages:
  - `Microsoft.SemanticKernel`
  - `Microsoft.SemanticKernel.Connectors.Ollama`
  - `Microsoft.SemanticKernel.Connectors.OpenAI`
  - `Microsoft.KernelMemory`
  - `Microsoft.Extensions.DependencyInjection`

---

## 💡 What this example does

### 🔹 Step 1: Ask LLaMA 3.2 directly
We ask the LLM a question:
> _"Do you know what the exchange rate is for 1 Euro compared to 1 Iranian rial?"_

This tests the LLM's answer without any external context (zero-shot).

---

### 🔹 Step 2: Use RAG with PDF Context
1. Configures **Semantic Kernel Memory** with Redis for storing embeddings.
2. Ingests a PDF document containing exchange rate information.
3. Creates a memory plugin to query the PDF content.
4. Constructs a custom prompt combining the user question and PDF context.
5. Executes the RAG-enhanced query using **Ollama-hosted LLaMA 3.2 (3B)**.

---

## 📎 Notes

- Replace `"your-redis-connection-string"` with your actual Redis connection string.
- Replace `"exchange_rates.pdf"` with the path to your PDF document.
- Adjust the prompt template or LLM settings as needed.

---
