# GitHub Copilot & Generative AI Instructions

---

## 1. Scope Control & Execution Constraints (Strict Prohibition)
- **Execution Process:** 1. Analyze for minimal changes -> 2. Verify simplicity (flatten conditionals, check nulls) -> 3. Execute required code.
- **Strict Prohibition:** NO speculative additions. NO "while we’re here" changes. Only implement what is explicitly asked. Do NOT modify unrelated code/files.
- **Output Constraints:** Produce ONLY required code. Avoid overengineering, generic abstractions, exploratory, or alternative implementations.
- **Out of scope enhancements:** If you intentionally restrain from out-of-scope enhancements, Gather all of them and document them above the method definition using multi-line comment section`/**/`, and then summarize all method-level comments above of the class definition using multi-line comment section `/* */`.
- **Edge Cases:** If you spot edge cases you did not implemented due to scope limitations, gather all of them and document them above the method definition using multi-line comment section`/**/`, and then summarize all method-level comments above of the class definition using multi-line comment section `/* */`.
- **Impliocations and possible risks:** If you any further implications or possible risks you did not implemented due to scope limitations, gather all of them and documentthem above the method definition using multi-line comment section`/**/`, and then summarize all method-level comments above of the class definition using multi-line comment section `/* */`.
- **Collaboration Discipline:** Suggest, don't apply automatically. Keep explanations <= 2 paragraphs. Preserve exact formatting.

## 2. Architecture: Razor Pages + Blazor Static SSR + HTMX
- **Target .NET 8** with ASP.NET Core cookie auth. Prioritize Blazor Static SSR for reusable blocks, Razor Pages for pages.
- **Blazor SSR:** Start with Static SSR components. Do not render vast HTML. Avoid interactive circuits unless demanded. Split markup/logic (`.razor` + `.razor.cs`). Keep pure (parameter in -> markup out). No `HttpClient` in components.
- **HTMX:** Return server partial views/fragments. Do NOT use inline JS for events (`onclick`); handle via HTMX partials. Prevent duplicate requests (`hx-trigger="click"`, `disabled`). Minimal transient per-request state.

## 3. C# & General Coding Standards (Scope-Limited)
- **Guard Clauses:** Validate nulls or invalid states immediately. Fail fast (return/throw) to avoid deep nesting.
- **Simplify Conditionals:** Avoid chained/deep `if-else`. Convert complex conditional trees to C# `switch` statements or expressions where applicable. Apply this ONLY to the active task scope.
- **Directives:** PascalCase public, `_camelCase` private. Explicit types. Single responsibility per method. Constructor DI only.
- **Async:** `async Task` ONLY. No `async void`, `.Result`, `.Wait()`.
- **Validation & Errors:** Inject `ILogger<T>`. Catch exceptions in services, not UI. Use FluentValidation.

## 4. Data Access, HTTP & UI
- **Data (EF Core):** Parameterized queries. Async methods. Explicit, short-lived transactions (no generic UoW). Run migrations on startup in limited envs. Always pass `CancellationToken`.
- **HTTP/Resilience:** `IHttpClientFactory` typed clients. Timeouts <= 10s. Polly retry (jitter) + circuit breaker. 
- **Frontend / UX:** Semantic HTML (`<main>`, `<button>`). PICO CSS (responsive prefixes). No hidden auth fields (rely on cookies). Minimal JS (in `/wwwroot/js/`). Inputs must have labels.

## 5. Repository Layout & Testing Policy
- **Layout:** `/Pages` (Razor), `/Components` (Blazor SSR), `/Services`, `/Data`, `/wwwroot` (Static JS/CSS), `/Migrations`.
- **Testing:** Unit (Services with fakes), Integration (HTMX endpoints return fragments), Components (bUnit for static render + accessibility).
