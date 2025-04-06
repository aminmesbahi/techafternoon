<div dir="rtl">

# مثال: استفاده از Semantic Kernel برای RAG با زمینه PDF

این مثال نشان می‌دهد که چگونه می‌توان از **Semantic Kernel** برای پیاده‌سازی یک سیستم RAG (بازیابی و تولید پاسخ) با استفاده از محتوای یک فایل PDF استفاده کرد. این سیستم از **مدل LLaMA 3.2 (3B)** میزبانی شده توسط Ollama برای تکمیل چت و **Redis** برای ذخیره حافظه استفاده می‌کند.

---

## 🔧 پیش‌نیازها

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- [Redis](https://redis.io/) برای ذخیره حافظه
- [Ollama](https://ollama.com/) با مدل `llama3.2:3b`
- یک فایل PDF حاوی اطلاعات نرخ ارز
- بسته‌های NuGet:
  - `Microsoft.SemanticKernel`
  - `Microsoft.SemanticKernel.Connectors.Ollama`
  - `Microsoft.SemanticKernel.Connectors.OpenAI`
  - `Microsoft.KernelMemory`
  - `Microsoft.Extensions.DependencyInjection`

---

## 💡 این مثال چه کاری انجام می‌دهد؟

### 🔹 مرحله 1: پرسش مستقیم از LLaMA 3.2
ما از مدل زبانی یک سوال می‌پرسیم:
> _"آیا می‌دانید نرخ تبدیل 1 یورو به ریال ایران چقدر است؟"_

این مرحله پاسخ مدل را بدون هیچ زمینه خارجی (zero-shot) آزمایش می‌کند.

---

### 🔹 مرحله 2: استفاده از RAG با زمینه PDF
1. **حافظه Semantic Kernel** را با Redis برای ذخیره embeddings پیکربندی می‌کند.
2. یک فایل PDF حاوی اطلاعات نرخ ارز را وارد می‌کند.
3. یک پلاگین حافظه برای جستجوی محتوای PDF ایجاد می‌کند.
4. یک prompt سفارشی شامل سوال کاربر و زمینه PDF می‌سازد.
5. پرسش RAG-enhanced را با استفاده از **مدل LLaMA 3.2 (3B)** اجرا می‌کند.

---

## 📎 نکات

- مقدار `"your-redis-connection-string"` را با رشته اتصال Redis خود جایگزین کنید.
- مقدار `"exchange_rates.pdf"` را با مسیر فایل PDF خود جایگزین کنید.
- قالب prompt یا تنظیمات مدل LLM را در صورت نیاز تغییر دهید.

---

</div>