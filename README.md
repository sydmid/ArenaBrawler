# Arena Brawler

> **A high-performance, real-time multiplayer arena brawler featuring a live trading economy.**

![.NET 9.0](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white&style=flat-square)
![Raylib](https://img.shields.io/badge/Raylib-cs-000000?logo=c-plus-plus&logoColor=white&style=flat-square)
![Oracle DB](https://img.shields.io/badge/Oracle_21c-F80000?logo=oracle&logoColor=white&style=flat-square)
![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white&style=flat-square)
![C#](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white&style=flat-square)

This repository serves as a showcase of advanced C# backend engineering, focusing on low-latency networking, memory optimization, and scalable server architecture.

## 🚀 Engineering Highlights

Designed for maximum performance and predictability, the architecture heavily prioritizes zero-allocation patterns and high-throughput data processing.

- **Zero-Allocation Networking:** Network payloads are strictly defined as unmanaged, 1-byte packed C# structs (`[StructLayout(LayoutKind.Sequential, Pack = 1)]`). All network serialization/deserialization is performed using `Span<byte>` and `MemoryMarshal` to guarantee **0 GC (Garbage Collection) overhead** during gameplay.
- **High-Frequency State Streaming:** Utilizes a custom `hf-state-streaming` library for state synchronization, ensuring deterministic and ultra-low latency client-server communication.
- **Live Trading Economy:** Implements an ultra-fast, in-memory order book for the live economy, engineered to handle a massive volume of concurrent transactions.
- **Asynchronous Persistence:** State is periodically and asynchronously persisted to an Oracle Database 21c using Dapper, ensuring data integrity without blocking the high-frequency game loop.

## 🏗️ Architecture

The system enforces a strict, authoritative **Client-Server architecture**:
- **Client:** A lightweight rendering and input-handling layer built with `Raylib-cs`.
- **Server:** A robust `.NET 9.0` Core server that acts as the single source of truth for game state and the trading economy.

## 🛠️ Quick Start

The infrastructure is fully containerized for a frictionless development experience.

**Prerequisites:**
- Docker & Docker Compose
- .NET 9.0 SDK
- PowerShell

**Run the Infrastructure:**
```powershell
# Spins up the required environment, including the gvenzl/oracle-xe:21-slim database
./scripts/run-containers.ps1
```

Once the containers are healthy, you can launch the server and client components directly via the .NET CLI or your preferred IDE.

---
*Built to demonstrate rigorous technical standards and systems-level C# engineering.*
