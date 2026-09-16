<div align="center">
  <h1>Arena Brawler</h1>
  <p><b>Real-Time Top-Down Multiplayer Arena Brawler with Integrated Live Trading Economy</b></p>
</div>

<br />

<div align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=.net" alt=".NET 9.0" />
  <img src="https://img.shields.io/badge/Raylib-6.0-000000?style=flat-square" alt="Raylib" />
  <img src="https://img.shields.io/badge/Database-Oracle%2021c-F80000?style=flat-square&logo=oracle" alt="Oracle 21c" />
  <img src="https://img.shields.io/badge/Deployment-Docker-2496ED?style=flat-square&logo=docker" alt="Docker" />
</div>

<br />

Welcome to **Arena Brawler**, an expertly engineered system demonstrating how to bridge low-latency authoritative physics simulation with a high-frequency financial order book economy.

This repository serves as the foundational architecture for game studios, backend engineers, and systems developers looking to construct robust multiplayer environments that natively integrate complex in-game economies without compromising on simulation tick rates.

---

## 🎯 The Vision: Gameplay Meets High-Frequency Trading

Arena Brawler is built around two core pillars:

1. **The Authoritative Simulation:** A 60 Hz tick-rate server written in pure C# .NET that calculates deterministic player combat, processes 0-GC unmanaged `Span<byte>` binary UDP payloads, and interpolates position deltas to the client.
2. **The Living Economy:** Rather than a simple storefront, the game features a fully integrated limit-order book (powered by `hf-state-streaming`). Players can execute swing trades on unique asset instances (skins, stickers, charms) in real-time, completely avoiding UI polling delays.

---

## ⚙️ Architectural Masterclass

### Zero-Allocation Binary Contracts
To guarantee maximum throughput, the client and server communicate via unmanaged, 1-byte packed structs. Network payloads are heavily optimized to prevent Garbage Collection pauses, ensuring absolute deterministic stability in the hot path.

### Hermetic Separation of Concerns
- **Client (`Game.Client`):** A lightweight `Raylib-cs` executable solely responsible for rendering, input polling, and local-state interpolation.
- **Server (`Game.Server`):** The unbreakable source of truth for both arena physics and market transactional integrity.

### High-Frequency Market & Persistence
Market data is streamed directly to clients via `hf-state-streaming`. To prevent thrashing the database, the C# matching engine executes trades synchronously in memory, queuing asynchronous LMAX Disruptor events to an Oracle 21c Database instance managed by `Dapper`.

---

## 🚀 Quick Start Guide

### Prerequisites
- **.NET 9.0 SDK** (Required for Native AOT and high-performance compilation)
- **Docker & Docker Compose** (For orchestrating the Oracle Database)
- **PowerShell** (For Windows deployment scripts)

### Launching the Infrastructure

1. **Spin up the Database & Server:**
   Execute the automated deployment script to build the .NET container and launch the Oracle 21c Database.
   ```powershell
   ./scripts/run-containers.ps1
   ```
2. **Run the Client:**
   Once the server indicates it is healthy, launch the desktop client from a new terminal:
   ```bash
   dotnet run --project src/Game.Client/Game.Client.csproj
   ```

---

## 📁 Repository Map

- `src/Game.Client`: The Raylib desktop executable and visual systems.
- `src/Game.Server`: The authoritative physics and matching engine loops.
- `src/Game.Shared`: Zero-allocation structs, schemas, and binary contracts.
- `lib/hf-state-streaming`: The core high-frequency market matching engine submodule.
- `scripts/`: Container orchestration, health checks, and Oracle DDL schemas.

---

<div align="center">
  <i>Engineered for low-latency scale.</i>
</div>
