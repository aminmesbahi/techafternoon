# Example: Using Semantic Kernel for Exchange Rate Processing with Semantic Memory

This example demonstrates how to use **Semantic Kernel** to implement a Retrieval-Augmented Generation (RAG) pipeline for processing exchange rates. The pipeline integrates **OpenAI GPT models** and **Semantic Kernel memory plugins** to enhance the LLM's responses with real-time exchange rate data.

---

## 🔧 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- Internet access to query real-time exchange rates
- NuGet packages:
  - `Microsoft.SemanticKernel`
  - `Microsoft.SemanticKernel.Connectors.OpenAI`
  - `Microsoft.Extensions.DependencyInjection`
  - `HtmlAgilityPack`

---

## 💡 What this example does

### 🔹 Step 1: Ask GPT directly
We ask the LLM a question:
> _"What is the exchange rate for 1 USD to EUR?"_

This tests the LLM's answer without any external context (zero-shot).

---

### 🔹 Step 2: Use RAG with Semantic Kernel
1. Scrapes the latest currency rates from **bonbast.org**
2. Stores those in Semantic Kernel's memory
3. Retrieves relevant entries from memory
4. Injects the results into a custom prompt
5. GPT generates a better-informed response using real-time data

---

## 📎 Notes

- You can customize the scraping logic to pull more or different data.
- Update the embedding model, prompt template, or LLM provider as needed.
- This example uses a dummy fallback for `GetExchangeRatesAsync()` if the site is not reachable (see comments in the code).

---
