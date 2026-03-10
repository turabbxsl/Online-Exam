# Online Exam System

An enterprise-grade, event-driven online examination platform built with ASP.NET Core.  
The system supports distributed orchestration, real-time exam monitoring, and high concurrency workloads.

The platform is designed to handle complex exam lifecycles while maintaining reliability, scalability, and fault tolerance.

---

## Architectural Foundation

The system follows a **distributed event-driven architecture** to coordinate different components and maintain system consistency during the exam lifecycle.

This architecture provides:

- Reliable processing of exam lifecycle events
- Loose coupling between system components
- Recovery capability if services restart during an active exam
- High availability during concurrent exam sessions

---

## Core Technical Stack

### Backend
- ASP.NET Core 8.0

### Distributed Messaging
- RabbitMQ – message broker for asynchronous communication

### Workflow Orchestration
- MassTransit
- Saga State Machine for managing long-running exam workflows

### Background Scheduling
- Quartz.NET for persistent and fault-tolerant job scheduling

### Data Access
- Dapper (Micro ORM) for high-performance query execution

### Database
- PostgreSQL as the primary persistence layer

### Real-Time Communication
- SignalR for low-latency communication between server and clients

### Frontend
- ASP.NET Core MVC (Server Side Rendering)
- Bootstrap 5.3
- Vanilla JavaScript

---

## Event-Driven Orchestration

The system uses an **event-driven communication model** where different parts of the system interact through domain events.

Examples of lifecycle events include:

- ExamStarted
- ExamFinished
- StudentAnswerSubmitted

These events are published to RabbitMQ and processed asynchronously by system components responsible for managing the exam workflow.

This approach ensures scalability and reduces direct dependencies between services.

---

## Distributed Workflow Management

Each exam session is represented as a **persistent Saga State Machine instance**.

The Saga coordinates the lifecycle of an exam session across multiple distributed components.

Responsibilities of the Saga include:

- Initializing exam session state
- Tracking student exam progress
- Managing exam state transitions
- Handling timeout scenarios
- Finalizing exam sessions when time expires

Because the Saga state is persisted, the workflow can recover safely even if a service restart occurs.

---

## Lifecycle Event Handling

RabbitMQ is used to publish and deliver exam lifecycle events across the system.

Examples include:

- ExamStarted
- ExamFinished

These events ensure reliable message delivery between distributed services and maintain consistency during the exam process.

---

## Background Task Scheduling

Exam timing and deadlines are managed using Quartz.NET.

When an exam session begins:

- Quartz schedules a persistent job with the exam deadline
- When the scheduled time is reached, Quartz triggers an event
- The event is handled by the Saga State Machine which finalizes the exam session

Quartz jobs are persisted in the PostgreSQL database.  
This ensures that scheduled deadlines remain intact even if the application restarts during an active exam.

---

## Key System Characteristics

- Event-driven distributed architecture
- Workflow orchestration using Saga State Machine
- Asynchronous messaging with RabbitMQ
- Persistent background scheduling with Quartz.NET
- Real-time exam status updates using SignalR
- High-performance database access with Dapper

## Exam Lifecycle Flow

### 1. Exam Initiation

**Student**

- Clicks the **StartExam** button.

**API**

- Receives the request and sends a `StartExamCommand` through MassTransit.

**Saga**

- Listens for the `ExamStarted` event.
- Creates a new **Saga Instance** (exam session) in the database.
- Sets the exam state to **InProgress**.

---

### 2. Deadline Scheduling

**Saga**

- Once the exam starts, the Saga requests Quartz.NET to schedule the exam deadline.

**Quartz.NET**

- Creates a **Deadline Job** in the database for the specific exam ID.
- This job stores the exact time when the exam should expire.

---

### 3. Active Exam Session

**Student**

- Selects answers during the exam.
- Each answer is published to RabbitMQ as a `StudentAnswerSubmitted` event.

**Saga**

- Consumes these events and updates the current exam session state.
- Tracks which questions have been answered and which remain unanswered.

**SignalR**

- Sends real-time notifications to the student's browser such as  
  *"Your answer has been recorded"*.

---

### 4. Timeout Trigger

**Quartz.NET**

- When the scheduled time arrives, the **Deadline Job** is triggered.

**Quartz.NET**

- Publishes an `ExamTimeoutEvent` to RabbitMQ.

---

### 5. Exam Finalization

**Saga**

- Consumes the `ExamTimeoutEvent`.
- Transitions the exam state to **Finished**.
- Calculates the final score (or triggers the appropriate scoring service).

**SignalR**

- Sends an `ExamFinished` notification to the student's browser.

**UI**

- The browser automatically redirects to the **Results Page**.

---

## Key Characteristics of the Workflow

### Event-Sourced Nature

The system does not directly update the database state through synchronous operations.  
Instead, state transitions occur through **domain events**.

This approach ensures that the entire exam lifecycle remains traceable and auditable.

---

### Persistent Recovery

If the server crashes or restarts, the system can safely recover.

Because both **Saga state** and **Quartz jobs** are persisted in PostgreSQL, the system can immediately detect expired exams and finalize them once it becomes available again.

---

### Decoupling

Each component in the system has a clearly defined responsibility:

- **API** issues commands
- **Saga** orchestrates the workflow
- **Quartz.NET** manages time-based triggers

This separation ensures that a failure in one component does not prevent other parts of the system from operating.

## Setup & Installation

Follow these steps to set up the local environment:

### 1. Database Setup
Ensure **PostgreSQL** is installed and running on your machine. Create a new database named `ExamSagaDb` and import the provided SQL dump file:

```bash
psql -U your_username -d ExamSagaDb -f ExamSagaDb.sql
```
### 2. Start RabbitMQ

Run RabbitMQ as a container using Docker:

```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 3. Application Configuration

Update the **appsettings.json** file in your projects with your specific environment details:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PostgreSql": "Host=YOUR_HOST;Port=YOUR_PORT;Database=ExamSagaDb;Username=YOUR_USERNAME;Password=YOUR_PASS"
  },
  "RabbitMq": {
    "Host": "YOUR_HOST"
  }
}
```
