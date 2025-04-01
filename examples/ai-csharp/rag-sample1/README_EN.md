# Example: Using Semantic Kernel for Exchange Rate Processing with Semantic Memory

This example demonstrates how to implement an intelligent Q&A system using **Semantic Kernel**. It utilizes a chat model (Ollama with model `llama3.2:3b`) to first answer the user's question directly and then, using the **RAG (Retrieval Augmented Generation)** technique with semantic memory, provides a more contextually enriched answer.

---

## Introduction to Semantic Kernel

**Semantic Kernel** is a development framework that enables the integration of large language models (LLMs) into applications. Its key features include:
- **Chat Completion:** Enabling interactive conversation-based responses.
- **Embeddings:** Mapping textual data into a semantic space.
- **Plugin Support:** Extending functionalities with additional components, such as **Semantic Memory**.

For more details, please refer to the [official Semantic Kernel documentation](https://github.com/microsoft/semantic-kernel).

---

## Understanding Semantic Memory

In this example, semantic memory is a key component to enhance response generation.
### Memory Structure:
- **VolatileMemoryStore:** A temporary (in-memory) storage used in this sample, which does not persist data.
- **SemanticTextMemory:** A class responsible for storing textual data along with its embeddings.

### Memory Lifecycle in a Chat Context:
1. **Memory Creation:** Semantic memory is created using an embedding generation service.
2. **Storing Data:** Exchange rate information (fetched from a website like bonbast.org) is saved into the memory.
3. **Data Retrieval:** When processing the final question, relevant information is recalled from the memory to serve as contextual input.
4. **Response Generation:** The retrieved memory data is combined with the original question in a prompt, and the chat model generates the final answer.

---

## Code Overview

1. **Initial Setup:**  
   - A primary question regarding the exchange rate between 1 Euro and 1 Iranian Rial is defined.
   - Two methods of response are provided: a direct answer from the chat model and an answer enhanced via the RAG technique.

2. **Kernel Initialization:**  
   - The code uses `Kernel.CreateBuilder()` to set up the environment and add the necessary services for chat completion and text embedding generation.
   - The Ollama model `llama3.2:3b` is registered as the chat and embedding provider.

3. **Direct Chat Response:**  
   - The question is sent to the model using `InvokePromptStreamingAsync`, and the response is streamed back to the user.

4. **Semantic Memory and RAG Technique:**  
   - The embedding service is retrieved, and a semantic memory instance (`SemanticTextMemory`) is created using a volatile memory store.
   - Exchange rate data is fetched from a specified URL, processed, and stored in the memory.
   - A memory plugin (`TextMemoryPlugin`) is imported into the kernel.
   - A new prompt is created that combines the original question with the retrieved memory data (using the placeholder `{{Recall}}`).
   - Finally, the model generates an enhanced answer based on the combined prompt.

---

## Step-by-Step Code Explanation

1. **Defining the Question:**  
   The primary question about the exchange rate between 1 Euro and 1 Iranian Rial is set up and displayed.

2. **Setting Up the Kernel and Services:**  
   - The kernel is built using `Kernel.CreateBuilder()`, and the Ollama chat model along with text embedding generation is configured.
   - The model is hosted locally and referenced by its identifier.

3. **Direct Response Execution:**  
   - The question is sent directly to the model via `kernel.InvokePromptStreamingAsync(question)`, which streams the answer back to the console.

4. **Preparing Semantic Memory:**  
   - The embedding generation service is obtained, and a new `SemanticTextMemory` is instantiated with a `VolatileMemoryStore`.
   - The code calls a helper method to scrape exchange rate data from a website (using HtmlAgilityPack), and each currency rate is stored in the memory with a unique key.

5. **Integrating Memory in RAG:**  
   - A memory plugin is created and imported into the kernel.
   - A composite prompt is constructed, which includes both the original question and the recalled memory data (denoted by `{{Recall}}`).
   - Additional arguments (like the currency pair) are set if found in the question.
   - Finally, the prompt is executed using `kernel.InvokePromptStreamingAsync`, streaming back an answer that leverages both the direct query and the contextual memory data.

This example clearly demonstrates how to utilize Semantic Kernel’s capabilities along with semantic memory to build an up-to-date, context-enriched Q&A system.

---

## Additional Resources

For the latest updates on Semantic Kernel and semantic memory concepts, refer to the [official Microsoft Semantic Kernel GitHub repository](https://github.com/microsoft/semantic-kernel).

---