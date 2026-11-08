# eShop - Enterprise Microservices Platform (.NET 10)

Enterprise-grade, cloud-native, event-driven eCommerce platform built with .NET 10, MassTransit, RabbitMQ, Docker, and SQL Server.

---

# 💡 The Golden Rules of Microservices

Building a production-grade distributed architecture requires a shift in mindset from traditional monolithic thinking. To maintain system uptime, data integrity, and engineering velocity, your architecture must strictly follow these **5 Golden Rules**:

---

### 1. Database Per Service (Strict Data Isolation)
* **The Rule:** Each microservice **must** completely own its own database (`eShop_OrderDb`, `eShop_InventoryDb`, etc.). 
* **The Constraint:** A microservice is strictly forbidden from directly connecting to, querying, or modifying another service’s database tables. 
* **Why it matters:** Direct cross-database joins create hard-coupling. If the Product team changes a column schema, it breaks the Order Service instantly. Data synchronization between services must happen *only* via public APIs or asynchronous integration events.

---

### 2. Asynchronous Event-Driven Communication over HTTP
* **The Rule:** Use message brokers (**RabbitMQ** via **MassTransit**) for cross-service transactions and downstream notifications. Avoid synchronous chaining (`Service A` calling `Service B` calling `Service C` over HTTP).
* **The Constraint:** Use HTTP/HTTPS (via **YARP API Gateway**) *only* for direct client-to-backend ingress requests or quick data lookups.
* **Why it matters:** Synchronous HTTP chains introduce cascading runtime failures. If the `NotificationService` goes offline or gets slow, it blocks the `PaymentService`, which in turn locks up checkout execution in the `OrderService`. Asynchronous queuing shields your system from network ripples.

---

### 3. Centralized Ingress (The API Gateway Pattern)
* **The Rule:** Frontend clients (React, Mobile apps) must communicate **exclusively** with a single entry point: the **YARP API Gateway**.
* **The Constraint:** Frontend clients must never talk directly to individual backend microservices or expose their internal port blocks.
* **Why it matters:** The Gateway handles cross-cutting concerns globally—such as centralizing CORS management, appending Correlation IDs, executing request rate-limiting, and managing routing paths. This keeps individual services clean, secure, and easily swap-able behind the firewall.

---

### 4. Protect Against the "Dual Write" Fallacy
* **The Rule:** Never write to a local database and publish an event to a message broker in the same standard code block without safety guarantees. You must use the **Transactional Outbox Pattern**.
* **The Constraint:** Persist the integration event inside your microservice's local database (e.g., `t_outbox_event`) within the exact same atomic transaction scope (`IUnitOfWork`) as your business data. 
* **Why it matters:** If your database successfully updates but your network blips right before sending the message to RabbitMQ, downstream services miss the change completely, leading to corrupted state values across your platform. A background publisher worker guarantees at-least-once message delivery.

---

### 5. Design for Concurrency and Eventual Consistency
* **The Rule:** Accept that data across distributed systems will be **eventually consistent**, and enforce defensive multi-user protections at the storage tier using **Optimistic Concurrency Control**.
* **The Constraint:** High-traffic services like inventory management must utilize row tracking mechanisms (such as SQL Server `ROWVERSION` byte tokens) inside your update filters (`WHERE [f_row_version] = @f_row_version`).
* **Why it matters:** In a microservices architecture, multiple instances of the same service run in parallel. Without database-level version verification checks, concurrent orders placed at the exact same millisecond will overwrite each other's updates, creating race conditions, negative balances, and data corruption.

# 1. Introduction & Architectural Vision

**eShop** is a complete reference blueprint implementation for building production-ready distributed systems using modern .NET technologies. The platform emphasizes true runtime decoupling, high data safety guarantees, and predictable performance layouts designed to mimic modern transactional scaling requirements.

### Core Architectural Principles
* **Microservices Design Pattern:** Highly cohesive, loosely coupled domain spaces with dedicated databases per service to prevent tight coupling.
* **Asynchronous Message Broker:** Event-driven choreographies utilizing **RabbitMQ** through **MassTransit** to decouple microservice inter-communications.
* **High-Throughput Data Persistence:** Lightweight ORM mapping with **Dapper** to enforce optimal query execution paths over SQL Server databases.
* **Gateway Level Decoupling:** Centralized ingress routing, CORS policies, and structural route mapping managed cleanly via **YARP Reverse Proxy**.
* **Hardened Concurrency Control:** System boundaries utilize **Optimistic Concurrency Control** via database row tokens to prevent race conditions during distributed operations.

---

# 2. Technology Stack

### Backend Core
* **Framework:** .NET 10 (ASP.NET Core 10 Web API)
* **Language:** C# 14
* **Data Access:** Dapper (Lightweight High-Performance ORM)
* **Primary Database:** Microsoft SQL Server 2022

### Messaging & Service Integration
* **Message Broker:** RabbitMQ (AMQP 0-9-1 Mesh Broker)
* **Abstraction Bus:** MassTransit (Distributed Application Framework)
* **API Gateway:** YARP (Yet Another Reverse Proxy)

### Containerization & Environment
* **Container Engine:** Docker Engine
* **Local Orchestration:** Docker Compose

### Observability & Quality Assurance
* **Logging Framework:** Serilog
* **Testing Engines:** xUnit, Moq

---

# 3. Database Design & Isolation Matrix

To maintain pure domain encapsulation, every single microservice owns its structural database container instance completely. Direct cross-database joins or unauthorized connections are strictly prohibited. Inter-service data synchronization is driven exclusively via event messages.

```text
  ┌─────────────────────────────────────────────────────────────┐
  │                   eShop Distributed Mesh                    │
  └─────────────────────────────────────────────────────────────┘
          │                 │                 │                 │
          ▼                 ▼                 ▼                 ▼
   [eShop_AuthDb]   [eShop_CartDb]    [eShop_OrderDb]   [eShop_InventoryDb]
   - Credentials    - Active Baskets  - Transactions    - Stock Records
   - Tokens & Roles - Price Snapshots - Outbox Ledger   - Row Concurrency
```

| Database Namespace | Storage Context Engine | Concurrency Pattern | Core Tables |
| :--- | :--- | :--- | :--- |
| **`eShop_AuthDb`** | SQL Server (dbo) | Optimistic / Unique | `t_user_credentials`, `t_refresh_token`, `t_role`, `t_user_role` |
| **`eShop_CartDb`** | SQL Server (dbo) | State-Replacement | `t_cart`, `t_cart_item` |
| **`eShop_OrderDb`** | SQL Server (dbo) | Transactional Outbox | `t_order`, `t_order_item`, `t_order_history`, `t_outbox_event` |
| **`eShop_InventoryDb`** | SQL Server (dbo) | **Optimistic (`ROWVERSION`)** | `t_inventory_stock`, `t_inventory_history` |
| **`eShop_CustomerDb`** | SQL Server (dbo) | Standard Row State | `t_customer`, `t_customer_address` |
| **`eShop_ProductDb`** | SQL Server (dbo) | Read-Heavy Optimizations | `t_product`, `t_product_category` |
| **`eShop_PaymentDb`** | SQL Server (dbo) | Transactional Audit Ledger | `t_payment` |
| **`eShop_ShippingDb`** | SQL Server (dbo) | State Tracking | `t_shipping_manifest`, `t_shipping_history` |
| **`eShop_NotificationDb`** | SQL Server (dbo) | Append-Only Tracking | `t_notification_history` |

# 4. Solution Architecture Template

The application source code follows an explicit, repeatable architectural convention split cleanly by layer responsibilities across all sub-folders:

```text
eShop Root Directory
│
├── 01. Gateways
│   └── eShop.APIGateway                  # Edge Routing Engine
│       ├── YARP Reverse Proxy
│       ├── Cross-Origin Resource Sharing (CORS) Policy
│       ├── Request Custom Routing
│       ├── Rate Limiting Middleware
│       ├── Correlation Id Propagation
│       └── Request Telemetry Logging
│
├── 02. Services
│   ├── AuthService                       # Identity Management Module
│   │   ├── eShop.AuthService.API
│   │   ├── eShop.AuthService.BusinessLogic
│   │   ├── eShop.AuthService.Database
│   │   ├── eShop.AuthService.Models
│   │   └── eShop.AuthService.Tests
│   │
│   ├── CustomerService                   # Client Profiles Module
│   │   ├── eShop.CustomerService.API
│   │   ├── eShop.CustomerService.BusinessLogic
│   │   ├── eShop.CustomerService.Database
│   │   ├── eShop.CustomerService.Models
│   │   └── eShop.CustomerService.Tests
│   │
│   ├── ProductService                    # Catalog Engine Module
│   │   ├── eShop.ProductService.API
│   │   ├── eShop.ProductService.BusinessLogic
│   │   ├── eShop.ProductService.Database
│   │   ├── eShop.ProductService.Models
│   │   └── eShop.ProductService.Tests
│   │
│   ├── CartService                       # Shopper Basket Interaction Module
│   │   ├── eShop.CartService.API
│   │   ├── eShop.CartService.BusinessLogic
│   │   ├── eShop.CartService.Database
│   │   ├── eShop.CartService.Models
│   │   └── eShop.CartService.Tests
│   │
│   ├── OrderService                      # Transaction Processing Module
│   │   ├── eShop.OrderService.API
│   │   ├── eShop.OrderService.BusinessLogic
│   │   ├── eShop.OrderService.Database
│   │   ├── eShop.OrderService.Models
│   │   └── eShop.OrderService.Tests
│   │
│   ├── PaymentService                    # Financial Transaction Ledger Module
│   │   ├── eShop.PaymentService.API
│   │   ├── eShop.PaymentService.BusinessLogic
│   │   ├── eShop.PaymentService.Database
│   │   ├── eShop.PaymentService.Models
│   │   └── eShop.PaymentService.Tests
│   │
│   ├── InventoryService                  # Warehouse Stock & Concurrency Module
│   │   ├── eShop.InventoryService.API
│   │   ├── eShop.InventoryService.BusinessLogic
│   │   ├── eShop.InventoryService.Database
│   │   ├── eShop.InventoryService.Models
│   │   └── eShop.InventoryService.Tests
│   │
│   └── NotificationService               # Background Notification Bus Worker
│       ├── eShop.NotificationService.Worker
│       └── eShop.NotificationService.Tests
│
└── 03. Infrastructure
    ├── eShop.Contracts                   # Shared Integration Contracts Boundary
    │   ├── Common
    │   │   ├── EventBase.cs
    │   │   └── IntegrationEvent.cs
    │   ├── Events
    │   │   ├── CustomerRegisteredEvent.cs
    │   │   ├── OrderSubmittedEvent.cs
    │   │   ├── OrderPaidEvent.cs
    │   │   ├── PaymentCompletedEvent.cs
    │   │   ├── InventoryReservedEvent.cs
    │   │   └── InventoryReleasedEvent.cs
    │   └── Enums
    │       ├── OrderStatus.cs
    │       └── PaymentStatus.cs
    │
    ├── eShop.EventBus                    # MassTransit Integration Layer
    │   ├── Extensions
    │   │   └── EventBusRegistrationExtensions.cs
    │   └── Serialization
    │       └── JsonSerializerHelper.cs
    │
    ├── eShop.Shared                      # Shared Persistence Engine Components
    │   ├── Common
    │   │   ├── BaseModel.cs
    │   │   ├── InvokeDataModel.cs
    │   │   └── Result.cs
    │   ├── Middleware
    │   │   ├── ExceptionMiddleware.cs
    │   │   └── CorrelationIdMiddleware.cs
    │   ├── Extensions
    │   │   └── SharedServiceExtensions.cs
    │   ├── UnitOfWork
    │   │   ├── IBaseRepository.cs
    │   │   ├── IUnitOfWork.cs
    │   │   └── UnitOfWork.cs
    │   └── Logging
    │       └── LogConstants.cs
    │
    └── eShop.Observability               # Diagnostic Instrumentation Layer
        ├── OpenTelemetry
        ├── Serilog Configuration
        ├── HealthChecks Endpoint Maps
        ├── Metrics Collectors
        └── Tracing Tracers
```


# 5. Implementation of RabbitMQ & MassTransit

The asynchronous event layer is decoupled using **MassTransit over RabbitMQ**, featuring an event-driven choreography pattern to safely broadcast transactional side-effects across the network mesh.

```text
 [Order Service]
       │
 (Saves Outbox Row)
       │
 [OutboxWorker] ───(Publish)───► [ RabbitMQ Exchange ]
                                         │
               ┌─────────────────────────┴────────────────────────┐
               ▼                                                  ▼
      [Cart Service Queue]                             [Notification Queue]
    (Clears Customer Cart)                          (Dispatches Message Alert)
```

### Message Contract Routing Table

| Broadcast Integration Event | Trigger Origin Module | Registered Consumers | Cluster System Impact |
| :--- | :--- | :--- | :--- |
| **`OrderSubmittedEvent`** | `OrderService` | `CartService`, `NotificationService` | Clears active shopper cart state; queues validation alerts. |
| **`OrderPaidEvent`** | `OrderService` | `NotificationService` | Dispatches invoice confirmation telemetry receipts. |
| **`PaymentCompletedEvent`** | `PaymentService` | `NotificationService` | Triggers downstream package assembly and labels. |
| **`CustomerRegisteredEvent`**| `CustomerService` | `NotificationService` | Dispatches platform introductory onboarding communications. |

### Reliability & Resiliency Policies
* **Message Retry:** All RabbitMQ configuration loops employ incremental retry rules (`Incremental(3, 5s, 5s)`) to handle transient database locks or network drops.
* **Transactional Outbox:** The `OrderService` avoids direct bus dependency writes by saving payloads locally to `t_outbox_event` under a single shared transaction scope (`IUnitOfWork`). The `OutboxPublisherWorker` background service polls, dispatches, and marks rows as completed to guarantee **At-Least-Once Delivery**.

---

# 6. Docker & Local Orchestration Topology

The development infrastructure is mapped out with a **sequential port schema strategy** to keep YARP Gateway configurations clear and eliminate random port mapping issues during local development.

### Network Topology Port Layout Matrix

```text
                          Inbound Internet Traffic (Port 7000)
                                           │
                                           ▼
                               ┌──────────────────────┐
                               │ YARP API Gateway Edge│
                               └──────────────────────┘
                                           │
       ┌───────────────┬───────────────────┼───────────────┬───────────────┐
       ▼               ▼                   ▼               ▼               ▼
  Auth Service   Cart Service      Customer Service  Order Service   Inventory Service
   Port 7010       Port 7020           Port 7030       Port 7050       Port 7040
```

| Microservice Module | Internal HTTP Port | Internal HTTPS Port | Gateway Entry Route Ingress Path |
| :--- | :---: | :---: | :--- |
| **YARP API Gateway** | **5000** | **7000** | Direct System Entry Face Point `/` |
| **Auth Service** | **5010** | **7010** | Proxy Route Target Path: `/api/auth/{**catch-all}` |
| **Cart Service** | **5020** | **7020** | Proxy Route Target Path: `/api/carts/{**catch-all}` |

| Customer Service | 5030 | 7030 | Proxy Route Target Path: /api/customers/{**catch-all} |
| Inventory Service | 5040 | 7040 | Proxy Route Target Path: /api/inventory/{**catch-all} |
| Order Service | 5050 | 7050 | Proxy Route Target Path: /api/orders/{**catch-all} |
| Payment Service | 5060 | 7060 | Proxy Route Target Path: /api/payments/{**catch-all} |
| Product Service | 5070 | 7070 | Proxy Route Target Path: /api/products/{**catch-all} |
| Shipping Service | 5080 | 7080 | Proxy Route Target Path: /api/shipping/{**catch-all} |

### Orchestrating with Docker Compose

The complete platform dependencies mesh is orchestrated using `docker-compose.yml` to ensure clean cross-container discovery:

```yaml
version: '3.8'

services:
  eshop.sqlserver:
    image: ://microsoft.com
    container_name: eshop.sqlserver
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourSecurePassword123!
    ports:
      - "1433:1433"
    networks:
      - eshop.mesh

  eshop.rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: eshop.rabbitmq
    ports:
      - "5672:5672"   # AMQP protocol port
      - "15672:15672" # Management Dashboard UI port
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    networks:
      - eshop.mesh

networks:
  eshop.mesh:
    driver: bridge
```

To initialize your complete database context dependencies, active messaging broker node instances, and logging systems locally, simply run:
```bash
docker-compose up -d
```

---

# 7. Deployment Blueprint

### Production Hardening Checklist
* **Environment Configuration Isolation:** Extract all secrets, API credentials, and SQL server connection strings into environment variables or use safe storage wrappers like Azure Key Vault or Kubernetes Secrets.
* **Zero-Downtime Deployments:** Run behind an active reverse proxy orchestration layer with continuous health checks (`/healthz` endpoints mapping active DB contexts) to prevent traffic drops during rollout phases.
* **Connection Multiplexing Tuning:** Increase internal HTTP pool allowances and Dapper query command timeouts inside production initialization variables to accommodate high volume spikes cleanly.