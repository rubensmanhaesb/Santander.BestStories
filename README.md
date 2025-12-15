# Santander Best Stories API (Hacker News)

This repository contains the implementation of the **Santander Backend Developer Coding Test**. The goal of the challenge is to expose a RESTful API that returns the **top N Hacker News stories**, ordered by **score (descending)**, following production‑ready backend practices.

The solution focuses on **clean architecture**, **testability**, **performance**, and **resilience**, as expected in a senior‑level backend assessment.

---

## 📌 Problem Statement

Using ASP.NET Core, implement a REST API that:

* Retrieves the list of **best story IDs** from the Hacker News API
* Fetches the **details of each story**
* Returns the **top N stories**, ordered by score (descending)
* Avoids overloading the public Hacker News API

Reference: [https://github.com/HackerNews/API](https://github.com/HackerNews/API)

---

## 🏗️ Architecture

The solution follows a layered (Clean Architecture‑inspired) structure:

```
Santander.BestStories
│
├── Api
│   └── Controllers and HTTP endpoints
│
├── Application
│   ├── Services (use cases)
│   ├── DTOs
│   ├── Abstractions (interfaces)
│   └── Options (configuration)
│
├── Infrastructure
│   ├── HTTP clients
│   ├── External integrations
│   └── Dependency Injection setup
│
└── Tests
    ├── Application.Tests
    └── Infrastructure.Tests
```

### Why this structure?

* Clear separation of concerns
* Business rules isolated from infrastructure
* Application layer fully testable without HTTP calls
* External dependencies easily replaceable

---

## 🚀 API Endpoint

### GET `/api/stories/best?n={number}`

Returns the **top N best stories**.

#### Query Parameters

| Name | Type | Description                 |
| ---- | ---- | --------------------------- |
| n    | int  | Number of stories to return |

#### Example Response

```json
[
  {
    "title": "A uBlock Origin update was rejected",
    "uri": "https://github.com/uBlockOrigin/...",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01.0000000+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

---

## ⚙️ Configuration

Configuration is handled via the **Options Pattern** using `HackerNewsOptions`:

```json
"HackerNews": {
  "BaseUrl": "https://hacker-news.firebaseio.com",
  "MaxN": 200,
  "PoolMultiplier": 2,
  "PoolMax": 500,
  "MaxConcurrentRequests": 8,
  "BestStoriesCacheTtl": "00:05:00",
  "ItemCacheTtl": "00:05:00"
}
```

### Configuration rationale

* **BaseUrl**: Explicit configuration to avoid hidden defaults
* **MaxN**: Prevents abusive or accidental large requests
* **Concurrency limits**: Protects the external API
* **Caching**: Reduces network calls and improves response time

---

## 🧠 Application Layer – Business Rules

The core logic is implemented in `BestStoriesService`:

* Validates input (`n <= 0`, `n > MaxN`)
* Retrieves and caches best story IDs
* Fetches story details concurrently using `SemaphoreSlim`
* Filters invalid or non‑story items
* Orders results by score (descending)
* Returns only the top N items

### Why `SemaphoreSlim`?

To explicitly control concurrency and avoid flooding the Hacker News API with simultaneous requests.

---

## 🧪 Testing Strategy

### ✔️ Application Tests

**Purpose:** Validate business rules independently of infrastructure.

Covered scenarios:

* Invalid `n` returns an empty result
* `n` is capped to `MaxN`
* Stories are ordered by score (descending)
* Null or non‑story items are ignored
* DTO mapping is validated
* Cache behavior is verified

Tools:

* xUnit
* Moq
* In‑memory cache (`MemoryCache`)

These are **pure unit tests**.

---

### ✔️ Infrastructure Tests

**Purpose:** Validate HTTP boundaries and serialization.

Approach:

* Custom `HttpMessageHandler` stub
* No real HTTP calls

Covered scenarios:

* Correct endpoint URLs
* JSON deserialization
* HTTP 404 and 500 behavior
* Mandatory configuration validation (`BaseUrl`)

Why no heavy mocking framework?

* Keeps tests close to real HTTP behavior
* Reduces brittleness

---

## 📌 Assumptions

The following assumptions were made while implementing this solution:

* The **Hacker News public API** is the single source of truth for story data.
* The external API may be slow or unavailable; therefore, **caching and concurrency limits** are mandatory.
* The API consumer should not be able to overload the system; input is capped via `MaxN`.
* HTTP errors (404, 500) from Hacker News are propagated as exceptions from the infrastructure layer and handled gracefully at higher layers.
* Story timestamps are returned in **ISO 8601 string format**, derived from Unix timestamps.
* This service is **read‑only** and does not persist data beyond in‑memory caching.
* The application is expected to run in a **stateless environment** (e.g., container or cloud service).

---

## 📦 Running the Application

### Prerequisites

* .NET SDK 9

### Run locally

```bash
dotnet restore
dotnet build
dotnet run --project Santander.BestStories.Api
```

### Run tests

```bash
dotnet test
```

---

## 🧩 Design Rationale

This solution prioritizes:

* Readability and maintainability
* Explicit configuration
* Test coverage at both business and infrastructure boundaries
* Production‑ready patterns

The code structure reflects how such a service would be implemented in a real enterprise environment.

---

## 📂 Public Repository Readiness

This repository is ready to be shared as a **public Git repository**, including:

* Clear setup and execution instructions
* Explicit assumptions and trade‑offs
* Comprehensive test coverage
* Clean and modular architecture

---

**Author:** Rubens Bernardes
