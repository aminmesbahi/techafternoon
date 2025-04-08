# RAG Enhanced Exchange Rate Lookup with Semantic Kernel and PostgreSQL

This example demonstrates how to implement an intelligent currency exchange rate lookup system using **Semantic Kernel**. It compares two approaches: a direct question to the LLM (`llama3.2:3b`) and an enhanced answer using the **RAG (Retrieval Augmented Generation)** technique with PostgreSQL vector storage.

---

## Introduction to Semantic Kernel

**Semantic Kernel** is a lightweight SDK that integrates Large Language Models (LLMs) into applications through plugins. Its key features include:
- **Chat Completion:** Enabling interactive conversation-based responses.
- **Embeddings:** Converting textual data into vector representations for semantic search.
- **Plugins:** Extending functionality with additional components like **Semantic Memory**.

For more details, please refer to the [official Semantic Kernel documentation](https://github.com/microsoft/semantic-kernel).

---

## Understanding RAG with pgvector

In this example, RAG is implemented using PostgreSQL with the pgvector extension:

### Memory Structure:
- **PostgresMemoryStore:** A persistent vector database using PostgreSQL with pgvector extension.
- **SemanticTextMemory:** A service that manages storing and retrieving textual data along with its vector embeddings.

### RAG Implementation Flow:
1. **Data Collection:** Exchange rate information is fetched from an external source (bonbast.org).
2. **Vector Generation:** Text embeddings are created for each exchange rate entry.
3. **Storage:** The embeddings and corresponding text are stored in pgvector.
4. **Query Processing:** When a question is received, relevant context is retrieved from the vector database.
5. **Enhanced Response:** The retrieved context is combined with the original question to generate a more accurate answer.

---

## Code Overview

1. **Initial Setup:**  
   - A question about the exchange rate between Euro and Iranian Rial is defined.
   - Two approaches are compared: direct LLM response vs. RAG-enhanced response.

2. **Kernel Configuration:**  
   - The Semantic Kernel builder pattern is used to set up necessary services.
   - Ollama is configured as both the chat completion and embedding generation provider.
   - The `llama3.2:3b` model is specified for both services.

3. **Direct LLM Response:**  
   - The question is sent directly to the model with minimal prompt engineering.
   - The response is streamed back to the console.

4. **RAG Implementation with pgvector:**  
   - A PostgreSQL connection is established with pgvector extension enabled.
   - The embedding service is configured with a vector size of 3072 (matching the model's embedding dimension).
   - Exchange rate data is fetched, processed, and stored in the vector database.
   - A specialized prompt template combines the user question with retrieved context.
   - The enhanced response leverages both the LLM's knowledge and the specific data retrieved.

---

## Step-by-Step Implementation Guide

1. **Prerequisites:**
   - PostgreSQL with pgvector extension installed
   - Ollama running locally with `llama3.2:3b` model
   - .NET SDK

2. **Database Setup:**
   ```sql
   CREATE EXTENSION vector;
   CREATE DATABASE ragdb;
   CREATE USER user WITH PASSWORD 'pass';
   GRANT ALL PRIVILEGES ON DATABASE ragdb TO user;
   ```

3. **Code Execution Flow:**
   - The application first demonstrates a direct response from the LLM.
   - After user confirmation (pressing Enter), it demonstrates the RAG approach.
   - Exchange rates are fetched and stored in the PostgreSQL vector database.
   - A specialized prompt template combines the question with retrieved context.
   - The enhanced response is generated and displayed.

4. **Key Components:**
   - **PostgresMemoryStore:** Manages vector storage in PostgreSQL.
   - **SemanticTextMemory:** Handles the saving and retrieval of information.
   - **TextMemoryPlugin:** Integrates memory capabilities with the kernel.
   - **OllamaPromptExecutionSettings:** Configures prompt execution parameters.

---

## Detailed Memory Integration

The key to this implementation is how memory is integrated:

```csharp
// Create pgvector memory store
var postgresMemoryStore = new PostgresMemoryStore(
    connectionString: connectionString,
    schema: schema,
    vectorSize: 3072
);

var memory = new SemanticTextMemory(postgresMemoryStore, embeddingGenerator);
var memoryPlugin = new TextMemoryPlugin(memory);
kernel.ImportPluginFromObject(memoryPlugin);
```

This creates a semantic memory system that:
1. Stores vector embeddings in PostgreSQL
2. Uses the Ollama model to generate embeddings
3. Makes memory operations available to the kernel via plugins

When the prompt is processed, the `{{Recall}}` placeholder is automatically populated with relevant information from the memory store.

---

## Performance Considerations

- **Vector Dimensions:** The example uses 3072-dimensional vectors, matching the embedding size of the `llama3.2:3b` model.
- **Relevance Threshold:** A minimum relevance score of 0.1 is set to ensure only meaningful data is retrieved.
- **Currency Pair Recognition:** The system identifies specific currency pairs in the question to improve retrieval accuracy.

---

## Additional Resources

- [Microsoft Semantic Kernel GitHub Repository](https://github.com/microsoft/semantic-kernel)
- [PostgreSQL pgvector Documentation](https://github.com/pgvector/pgvector)
- [Ollama Project](https://ollama.ai/)

---