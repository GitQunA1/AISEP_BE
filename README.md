<p align="center">
  <h1 align="center">🚀 AISEP — AI-powered Startup Ecosystem Platform (Backend)</h1>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/PostgreSQL-16-336791?logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/EF%20Core-8.0-512BD4?logo=dotnet&logoColor=white" alt="EF Core" />
  <img src="https://img.shields.io/badge/OpenAI-GPT-412991?logo=openai&logoColor=white" alt="OpenAI" />
  <img src="https://img.shields.io/badge/Ethereum-Sepolia-3C3C3D?logo=ethereum&logoColor=white" alt="Ethereum Sepolia" />
  <img src="https://img.shields.io/badge/SignalR-Realtime-512BD4?logo=dotnet&logoColor=white" alt="SignalR" />
  <img src="https://img.shields.io/badge/Cloudinary-Storage-3448C5?logo=cloudinary&logoColor=white" alt="Cloudinary" />
  <img src="https://img.shields.io/badge/AWS-Cloud-FF9900?logo=amazonaws&logoColor=white" alt="AWS" />
  <img src="https://img.shields.io/badge/Cloudflare-Tunnel-F38020?logo=cloudflare&logoColor=white" alt="Cloudflare" />
  <img src="https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white" alt="Docker" />
</p>

---

## 📖 Introduction

**AISEP** (AI-powered Startup Ecosystem Platform) is a comprehensive backend service that bridges the gap between **Startups**, **Investors**, and **Advisors** within a single, trust-driven ecosystem.

### The Problem

The startup ecosystem is fragmented: founders struggle to find credible investors, investors lack transparent tools to evaluate startups at scale, and advisors have no streamlined way to manage consultations or earn fair payouts. Document forgery and lack of verifiable due-diligence further erode trust.

### The Solution

AISEP provides a **unified platform** where:

- **Startups** create and submit projects with blockchain-verified documents, receive AI-powered scoring, and connect with investors.
- **Investors** discover vetted projects, request AI-driven analysis, negotiate deals with on-chain integrity, and unlock premium startup profiles.
- **Advisors** are auto-assigned to projects based on industry expertise, manage consulting bookings, submit reports, and receive monthly payouts.
- **Staff & Admins** oversee approvals, manage commissions, monitor user reports, and govern platform configuration.

All sensitive documents are **hashed and registered on the Ethereum Sepolia blockchain** for tamper-proof verification, while **OpenAI** powers intelligent startup/investor scoring and analysis.

---

## ✨ Key Features

| Module | Description |
|---|---|
| **🔐 Authentication & Authorization** | JWT-based auth with refresh tokens, role-based access control (Startup, Investor, Advisor, Staff, Admin), email verification via SMTP/MailKit |
| **📄 Document Management & Blockchain Verification** | Upload project documents to Cloudinary, compute SHA hash, register on Ethereum Sepolia smart contract, verify integrity on-chain anytime |
| **🤖 AI-Powered Analysis (OpenAI)** | Startup project scoring (potential, market, team), Investor profile analysis, scorecard-weight-based hybrid scoring, PDF extraction for automated data ingestion |
| **💬 Real-time Communication (SignalR)** | Live notifications hub, real-time chat between connected users (Startup ↔ Investor, Advisor ↔ Startup) |
| **💰 Payments & Wallet (SePay / VietQR)** | Subscription purchases, booking payments via VietQR bank transfer webhook, internal wallet system, platform commissions |
| **📊 Deal Flow Management** | Investor–Startup deal creation, three-step sequential signing, blockchain tracking for deal integrity |
| **📅 Booking & Consulting** | Advisor availability management, time-slot booking with free-quota and premium flows, consulting report submission with deadlines |
| **🏦 Payout System** | Monthly advisor payout groups, staff approval/rejection, bank account management, retry-request flow |
| **🔗 Connections & Networking** | Investor–Startup connection requests, auto-open chat on acceptance, project follower system |
| **📝 Project Lifecycle** | Draft → Pending → Approved/Rejected workflow, dynamic form validation (DB-driven), auto advisor assignment |
| **⚙️ Admin Configuration** | System commission configs, scorecard weight tuning, due-diligence templates, form validation rules, industry & stage options, system terms management |
| **🔔 Notifications** | Persistent notification storage + real-time SignalR push to connected clients |
| **📑 Subscription & Packages** | Role-targeted packages (Startup/Investor), quota-based project view unlocking, bonus free bookings |
| **🛡️ User Reports & Moderation** | Booking-bound user reports with evidence upload, resolution metadata, admin moderation |
| **🔄 Background Services** | Subscription expiry checker, booking response expiry, consulting report deadline enforcer, auto advisor assignment, blockchain ownership assignment queue |

---

## 🛠️ Tech Stack & Architecture

### Core Technologies

| Category | Technology |
|---|---|
| **Runtime** | .NET 8 (ASP.NET Core Web API) |
| **Language** | C# 12 |
| **Database** | PostgreSQL (via Npgsql) |
| **ORM** | Entity Framework Core 8 (Code-First) |
| **Authentication** | ASP.NET Core Identity + JWT Bearer Tokens |
| **Real-time** | ASP.NET Core SignalR |
| **Validation** | FluentValidation |
| **Object Mapping** | AutoMapper |
| **Filtering/Paging** | Sieve |
| **Blockchain** | Nethereum (Ethereum Sepolia via Infura) |
| **AI / LLM** | OpenAI API (GPT models) |
| **File Storage** | Cloudinary |
| **Email** | MailKit (SMTP) |
| **PDF Processing** | PdfPig (text extraction), QuestPDF (report generation) |
| **Payment Gateway** | SePay (VietQR bank transfer webhook) |
| **API Docs** | Swagger / Swashbuckle |
| **Containerization** | Docker (multi-stage build) |

### Infrastructure & Deployment

| Category | Technology / Service |
|---|---|
| **Cloud Provider** | **AWS** — Both the Backend API server and the PostgreSQL database are hosted on AWS EC2/RDS instances (bootstrapped with the AWS $100 free credit). |
| **Network Security** | **Cloudflare Tunnel** — The server is securely exposed to the internet through a Cloudflare Tunnel. This eliminates the need to open inbound firewall ports and provides built-in DDoS protection, bot mitigation, and TLS termination. |
| **Production Domain** | **`aisep.tech`** — The production API is reachable at `https://aisep.tech`. The tunnel routes all traffic from this domain directly to the Docker container running on the AWS instance. |

```
  [Client / Frontend]  →  https://aisep.tech
          │
          ▼
  ┌───────────────────┐
  │  Cloudflare Edge  │  ← DDoS protection, TLS, WAF
  └───────────────────┘
          │  (Cloudflare Tunnel — outbound only)
          ▼
  ┌───────────────────────────────────────┐
  │           AWS EC2 Instance            │
  │  ┌─────────────────────────────────┐  │
  │  │  Docker: AISEP.API (port 8080)  │  │
  │  └─────────────────────────────────┘  │
  │  ┌─────────────────────────────────┐  │
  │  │  PostgreSQL Database (port 5432)│  │
  │  └─────────────────────────────────┘  │
  └───────────────────────────────────────┘
```

### Application Architecture

The project follows a **classic 3-layer architecture**:

```
┌─────────────────────────────────────────────────┐
│                   API Layer                     │
│  (Controllers, Hubs, Middleware, Program.cs)     │
├─────────────────────────────────────────────────┤
│             Business Logic Layer (BLL)          │
│  (Services, DTOs, Validators, Helpers, Settings)│
├─────────────────────────────────────────────────┤
│              Data Access Layer (DAL)            │
│  (Entities, Repositories, DbContext, Migrations)│
└─────────────────────────────────────────────────┘
```

Communication pattern: **Controller → Service → Repository → DbContext**

---

## 📁 Project Structure

```
AISEP_BE/
├── .github/workflows/         # CI/CD pipeline definitions
├── AISEP/
│   ├── AISEP.sln              # Solution file
│   ├── *.puml                 # PlantUML sequence & class diagrams
│   │
│   ├── src/
│   │   ├── AISEP.API/                  # 🌐 API Layer (Entry Point)
│   │   │   ├── Controllers/            #   39 API controllers
│   │   │   ├── Hubs/                   #   SignalR hubs (Chat, Notifications)
│   │   │   ├── Realtime/               #   SignalR notification publisher
│   │   │   ├── Middleware/             #   Global exception handler
│   │   │   ├── ContractABI.json        #   Ethereum smart contract ABI
│   │   │   ├── Dockerfile              #   Multi-stage Docker build
│   │   │   ├── Program.cs              #   DI, Auth, CORS, pipeline config
│   │   │   ├── appsettings.json        #   🔒 Real secrets — Git-ignored!
│   │   │   └── appsettings.example.json  #   Template with dummy values — committed to Git
│   │   │
│   │   ├── AISEP.BLL/                  # 🧠 Business Logic Layer
│   │   │   ├── Services/              #   41 service modules
│   │   │   │   ├── AI/                #     OpenAI, Startup/Investor analysis
│   │   │   │   ├── Auth/              #     Authentication & registration
│   │   │   │   ├── Blockchain/        #     Smart contract interactions
│   │   │   │   ├── Payments/          #     SePay webhook & payment logic
│   │   │   │   ├── Chats/             #     Chat session & messages
│   │   │   │   ├── Deals/             #     Investment deal lifecycle
│   │   │   │   ├── BackgroundServices/#     Hosted background workers (5)
│   │   │   │   └── ...               #     (+ 34 more service modules)
│   │   │   ├── DTOs/                  #   Request & Response models
│   │   │   ├── Validators/            #   FluentValidation rules (24 modules)
│   │   │   ├── Helpers/               #   AutoMapper profiles, pagination, scoring
│   │   │   ├── Settings/              #   Strongly-typed config classes (6)
│   │   │   └── Exceptions/            #   Custom exception types
│   │   │
│   │   └── AISEP.DAL/                  # 💾 Data Access Layer
│   │       ├── Entities/              #   46 entity models
│   │       ├── Enums/                 #   35 enum definitions
│   │       ├── Repositories/          #   40 repository modules
│   │       ├── Data/                  #   ApplicationDbContext (EF Core)
│   │       ├── Common/                #   Unit of Work pattern
│   │       └── Migrations/            #   EF Core migration history (90+)
│   │
│   └── tests/
│       └── AISEP.BLL.Tests/           # 🧪 Unit tests for BLL
│
├── .gitignore
└── README.md                          # 📖 You are here
```

---

## 📋 Prerequisites

Before running the project, ensure you have the following installed:

| Tool | Version | Download |
|---|---|---|
| **.NET 8 SDK** | 8.0+ | [Download](https://dotnet.microsoft.com/download/dotnet/8.0) |
| **PostgreSQL** | 14+ | [Download](https://www.postgresql.org/download/) |
| **Git** | Any | [Download](https://git-scm.com/downloads) |
| **Docker** *(optional)* | 20.10+ | [Download](https://www.docker.com/products/docker-desktop) |
| **IDE** *(recommended)* | — | Visual Studio 2022 / VS Code / Rider |

### Third-Party Accounts (for full functionality)

| Service | Purpose | Sign Up |
|---|---|---|
| **Cloudinary** | File/image storage | [cloudinary.com](https://cloudinary.com/) |
| **OpenAI** | AI analysis (GPT API) | [platform.openai.com](https://platform.openai.com/) |
| **Infura** | Ethereum Sepolia RPC access | [infura.io](https://infura.io/) |
| **Gmail** | SMTP email sending | Use an App Password from [Google Account](https://myaccount.google.com/apppasswords) |
| **SePay** | Payment webhook (VietQR) | [sepay.vn](https://sepay.vn/) |

---

## 🚀 Step-by-Step Setup Guide

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/AISEP_BE.git
cd AISEP_BE
```

### 2. Set Up PostgreSQL Database

Create a new PostgreSQL database:

```sql
-- Connect to PostgreSQL (psql, pgAdmin, or DBeaver)
CREATE DATABASE "aisepDB";
```

> **Note:** The default connection string uses `localhost:5432`, username `postgres`, password `12345`. Adjust to match your local setup.

### 3. Configure Application Settings

The repository includes `appsettings.example.json` — a safe template with dummy/placeholder values committed to Git. The real `appsettings.json` (containing actual secrets) is **gitignored** and never pushed to GitHub.

Copy the template and fill in your own credentials:

```bash
cd AISEP/src/AISEP.API
cp appsettings.example.json appsettings.json
```

Then open `appsettings.json` and replace every `YOUR_..._HERE` placeholder with your actual values.

### 4. Restore NuGet Packages

```bash
cd AISEP
dotnet restore AISEP.sln
```

### 5. Apply EF Core Migrations

Run the following command from the **solution root** (`AISEP/`) directory:

```bash
dotnet ef database update \
  --project src/AISEP.DAL/AISEP.DAL.csproj \
  --startup-project src/AISEP.API/AISEP.API.csproj
```

> **Windows PowerShell** — use backtick `` ` `` for line continuation:
> ```powershell
> dotnet ef database update `
>   --project src/AISEP.DAL/AISEP.DAL.csproj `
>   --startup-project src/AISEP.API/AISEP.API.csproj
> ```

This will apply all 90+ migrations and create the full database schema.

### 6. Run the Application

```bash
cd src/AISEP.API
dotnet run
```

Or with hot-reload:

```bash
dotnet watch run --project src/AISEP.API/AISEP.API.csproj
```

The API will start on:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001`

### 7. Access Swagger UI

Open your browser and navigate to:

```
https://localhost:5001/swagger
```

You will see the full interactive API documentation with all 39 controllers.

### 8. (Optional) Run with Docker

```bash
cd AISEP/src/AISEP.API

docker build -t aisep-api .
docker run -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Host=host.docker.internal;Port=5432;Database=aisepDB;Username=postgres;Password=12345" \
  aisep-api
```

> Make sure your PostgreSQL is accessible from the Docker container. Use `host.docker.internal` on Docker Desktop.

### 9. Run Tests

```bash
cd AISEP
dotnet test tests/AISEP.BLL.Tests/AISEP.BLL.Tests.csproj
```

---

## 🔗 API Endpoints Overview

The API exposes **39 controllers** organized into the following groups:

| Group | Controllers | Key Endpoints |
|---|---|---|
| **Auth** | `AuthController` | `POST /api/auth/login`, `POST /api/auth/register`, `POST /api/auth/refresh-token` |
| **Users** | `UsersController`, `AdminController` | `GET /api/users/me`, `PATCH /api/admin/users/{id}/status` |
| **Startups** | `StartupsController` | `GET /api/startups`, `POST /api/startups`, `PUT /api/startups` |
| **Investors** | `InvestorController` | `GET /api/investors`, `POST /api/investors` |
| **Projects** | `ProjectsController`, `ProjectFollowerController` | `POST /api/projects`, `PATCH /api/projects/{id}/submit` |
| **Documents** | `DocumentController` | `POST /api/projects/{id}/documents`, `GET /api/documents/{id}/verify` |
| **AI Analysis** | `StartupAIAnalysisController`, `InvestorAIAnalysisController` | `POST /api/startup-ai-analysis`, `POST /api/investor-ai-analysis` |
| **Deals** | `DealsController` | `POST /api/deals`, `PATCH /api/deals/{id}/sign` |
| **Bookings** | `BookingController` | `POST /api/bookings`, `PATCH /api/bookings/{id}/approve` |
| **Payments** | `PaymentController`, `WalletController`, `TransactionsController` | `POST /api/payments/sepay-webhook` |
| **Chat** | `ChatSessionController`, `ChatMessageController` | `GET /api/chat-sessions`, `POST /api/chat-messages` |
| **Notifications** | `NotificationsController` | `GET /api/notifications` |
| **Connections** | `ConnectionsController` | `POST /api/connections`, `PATCH /api/connections/{id}/accept` |
| **Advisors** | `AdvisorController`, `AdvisorAvailabilityController`, `AdvisorBankAccountsController` | `POST /api/advisors`, `GET /api/advisor-availability` |
| **Payouts** | `PayoutController`, `PayoutGroupsController` | `GET /api/payouts/me`, `POST /api/payout-groups/generate` |
| **Admin Config** | `AdminScorecardConfigController`, `AdminDueDiligenceTemplateController`, `AdminTermsController`, `SystemCommissionController`, `FormValidationRulesController` | Various admin CRUD endpoints |
| **Other** | `ReviewController`, `PostPrsController`, `SubscriptionsController`, `ConsultingReportController`, `UserReportsController`, `EnumController`, `IndustryOptionsController`, `StageOptionsController`, `TermsController` | Various |

---

## 📡 SignalR Hubs

| Hub | Endpoint | Purpose |
|---|---|---|
| **NotificationHub** | `/hubs/notifications` | Real-time notification push to authenticated users |
| **ChatHub** | `/hubs/chat` | Real-time chat messaging between connected users |

Connect with `access_token` query parameter for JWT authentication.

---

## 🔧 Background Services

The application runs **5 hosted background services**:

| Service | Interval | Purpose |
|---|---|---|
| `SubscriptionExpiryBackgroundService` | Periodic | Expires overdue subscriptions and resets quotas |
| `BookingResponseExpiryBackgroundService` | Periodic | Auto-expires unresponded booking requests |
| `ConsultingReportDeadlineBackgroundService` | Periodic | Enforces consulting report submission deadlines |
| `ProjectAdvisorAutoAssignBackgroundService` | Every 1 min | Auto-assigns advisors to approved projects by industry match |
| `BlockchainOwnershipAssignmentBackgroundService` | Queue-based | Processes blockchain document ownership assignments |

---

## 📝 Configuration & Environment Variables

### Configuration File Strategy

`appsettings.json` holds the real credentials and is **gitignored** — it is never pushed to GitHub. The repository instead ships `appsettings.example.json` as a safe template (with dummy values only) so new developers can clone the repo, copy the example file, rename it to `appsettings.json`, and fill in their own credentials.

```
appsettings.example.json   ← committed to Git   (dummy values — safe to share)
appsettings.json           ← Git-ignored         (your real secrets — local only)
```

### Production on AWS (Docker + Cloudflare Tunnel)

On the AWS EC2 instance, sensitive settings are passed via environment variables at Docker runtime, keeping no secret files on disk:

```bash
docker run -p 8080:8080 \
  -e "ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=aisepDB;..." \
  -e "JwtSettings__SecretKey=your-production-secret" \
  -e "OpenAISettings__ApiKey=sk-..." \
  aisep-api
```

Traffic is then routed from `https://aisep.tech` through the **Cloudflare Tunnel** to the container's port `8080` — no inbound firewall ports need to be opened on the EC2 instance.

---

## 🤝 Contributing

1. Create a feature branch from `main`
2. Follow the existing code patterns (Service → Interface, DTO in/out, FluentValidation)
3. Add unit tests in `tests/AISEP.BLL.Tests/`
4. Run `dotnet build` and `dotnet test` before pushing
5. Open a Pull Request with a clear description

---

## 📄 License

This project is developed as part of **SEP490** — a capstone project. Contact the team for licensing information.

---

<p align="center">
  Built with ❤️ by the AISEP Team
</p>
