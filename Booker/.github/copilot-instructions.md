# GitHub Copilot & Generative AI Instructions

---
# Main technical ideas
- We want to use Razor Pages, Blazor Static SSR components for the initial render and then progressively enhance with HTMX for dynamic loading,
hybrid approach that gives us the best of both worlds: SEO-friendly server rendering + dynamic client interactions.


## 1. Meta-Instructions for AI Assistants

### 1.1 General Principles

- Creating list of steps, put substeps to new line with additional indentation.
- Group approaches by general strategy - under separater h2 header, ask at the end which one to implement.
- For sub-steps use letters minor numbering (1.1, 1.2, etc), put substeps on new lines.
- Ask me whether i want to see snippets and for any approach, do not show them automaticaly.
- Snippet sections start with h2 header with approach it implements.
- Try always create simplest solution that meets the requirements, any additional spotted or possible complexity adress as a follow up suggestion.
- When creating code snippets, and steps for implement a aproach, do not inlucde optional steps, list them as suggestions after the main steps.
- Preserve formatting and indentation exactly.  
- Skip the step dotnet run in order to test your changes, unless specifically asked for it.

### 1.2 Collaboration Discipline
- **Suggest, don’t apply:** list extra ideas as bullets after code.  
- ≤ 2 paragraphs explanation.  
- The developer is the final authority.

---

## 2. Project Overview & Core Principles

You are an expert AI programming assistant.  
Primary goal → help build a **secure, maintainable, performant Razor Pages + Blazor Static SSR + htmx application**.

### 🔒 Security First
- Server-side auth & authorization.  
- Use ASP.NET Core cookie auth (Windows Auth or OIDC when introduced).   

### ⚡ Performance by Default
- Use async/await correctly; avoid blocking calls.  
- Enable ADO.NET connection pooling.  
- Keep Razor/Blazor partials small; stream responses where practical.

### 🧩 Maintainability & Readability
- Follow **SOLID** and **KISS**.  
- Keep each method focused; prefer explicit, self-documenting code.

### ⚙️ Consistency
- Rules are **absolute** unless explicitly overridden.  
- When unsure → ask, don’t guess.


## 4. Blazor Static SSR & HTMX

### 4.1 Blazor SSR Strategy
- For Blazor related stuff start with **Static SSR** components
- Do not render lot of html on the server side with Blazor, use htmx.
- Prefer Razor Pages for full pages; Blazor Static SSR for reusable UI parts.
- Prefer Blazor interactivity over JS for islands of interactivity.
- Avoid accidental interactive circuits unless a feature demands it.
- Split markup/logic (`.razor` + `.razor.cs`).  
- Prefer parameters over cascading state unless necessary.  
- No direct HTTP calls inside components; use services.
- Keep components pure (parameter in → markup out).  
- Avoid heavy per-request allocation; prefer streaming partial HTML when possible.
- No `HttpClient` in Razor/Blazor components; only in services.

### 4.2 HTMX Strategy

- Do not put javascript in the htmx events like onclick, onsubmit, etc, create partial view or component to handle the htmx request on the server side.
- Do not put javascript for any htmx attributes like hx-get, hx-post, hx-swap, create partial view or component to handle the htmx request on the server side.
- Server endpoints return partial Razor views or fragments (no full layouts).  
- Include `hx-headers` only for non-sensitive metadata; auth handled by cookies.  
- Prevent duplicate requests with `hx-trigger="click"` + `disabled` state handling.  
- Progressive enhancement: prefer htmx over JS for interactivity.
- Use `hx-indicator` for loading spinners; ensure accessible announcements.

### 4.3 State
- Minimal transient per-request state; avoid long-lived interactive state.  
- If interactive islands added, scope them narrowly.

### 4.4 Error Handling
- Use `ErrorBoundary` only if interactive islands introduced; otherwise rely on Razor Pages error pipeline.

---

## 5. C# and Backend Directives

- PascalCase public; `_camelCase` private.  
- Use explicit types unless trivially obvious.  
- Always use braces.  
- Single Responsibility per method.

### Async Discipline
- `async Task` / `Task<T>` only.  
- No `async void`, `.Result`, `.Wait()`.

### Dependency Injection
- Constructor DI only.  
- Scoped → repositories/services.  
- Transient → DB connections.  
- Singleton → config/cache.

### Logging & Validation
- Inject `ILogger<T>`.  
- Use FluentValidation.  
- Catch exceptions in services, not UI.

---

## 6. Data Access & Migrations

### 6.1 EntityFramework
- Parameterized queries only.  
- Use async methods

### 6.2 Migrations 
- raw SQL last resort.  
- Run migrations on startup in controlled envs.  
- Seed data via migrations (dev-only flags).

### 6.3 Data Guardrails
- Always pass `CancellationToken`.  
- Connection pooling via connection string settings.  
- Explicit, short-lived transactions; no generic UoW.

---

## 7. Frontend & Accessibility

### 7.1 HTML (Semantic + Secure)
- Use semantic elements: `<main>`, `<nav>`, `<header>`, `<footer>`, `<section>`, `<article>`.  
- `<button>` for actions; avoid clickable non-semantic elements. 
- No hidden auth fields; rely on cookies.

### 7.2 PICO CSS
- Prefer responsive prefixes (`sm: md: lg:`).  
- No magic numbers;   

### 7.3 JavaScript
- Minimal JS; only enhancement.  
- Files in `/wwwroot/js/`; no inline `<script>`.  
- `'use strict';` at top.  
- No direct fetch/XHR for authenticated data if server can render HTML partials.

### 7.5 Accessibility & UX
- All inputs have labels.  
- Manage focus after htmx swaps (set `hx-trigger` + JS focus helpers).  
- String literals in resources for localization.

---

## 8. Visual Studio & Tooling

### 8.1 Assistant Output
- Include file path when proposing edits.  

### 8.2 IDE & Build Configuration
- Enable .NET analyzers;
- Nullable enabled.  
---

## 9. Repository Layout

- Dependencies — Project references and NuGet packages (node, not a folder).
- Properties — Project settings (e.g., `launchSettings.json`), assembly info.
- wwwroot — Public static files (CSS, JS, images) served directly.
- Areas — Feature areas with their own `Pages/` and routing; optional.
- Authorization — Policies, requirements, handlers, and related helpers.
- Data — Persistence types (DbContext, repositories, seeders).
- Migrations — EF Core migration snapshots and scripts.
- Pages — Razor Pages (`*.cshtml` + `*.cshtml.cs`) and folders per feature.
- Resources — Localization resources (`.resx`) and shared strings.
- Services — Application/UI-layer services registered via DI.
- TagHelpers — Reusable Razor Tag Helpers for UI concerns.
- Utilities — Cross-cutting helpers and extensions (e.g., guards, helpers).
- Components - Blazor components

## 10. Configuration & Environment

### 10.1 Environment Modes

### 10.2 Secrets & Configuration
- Use __Project > Manage User Secrets__ locally.  
- Precedence: `appsettings.json` → `appsettings.{Environment}.json` → env vars → user secrets.  
- Feature flags: `FeatureFlags:EnableDevSeeding`.

---

## 11. Outbound HTTP & Resilience

- `IHttpClientFactory` typed clients.  
- Timeouts ≤ 10s; retry (jitter) + circuit breaker + timeout via Polly.  
- Pass `CancellationToken` through layers.  

---

## 12. Error Handling, Logging, Telemetry

- Services translate exceptions to results; UI displays outcomes.  
- Structured logging (`ILogger<T>`), no PII.  
- Correlation id added per request.  
- Serilog optional sinks; Info in prod, Debug in dev.  
- FluentValidation in Application/Service layers only.

---

## 13. Testing Policy

- Unit: services with fakes; real logic only.  
- Integration: htmx endpoints return expected fragments.    
- Components: bUnit for SSR components; verify static render + accessibility.  
- htmx: integration tests assert partial HTML shape & cache headers.
