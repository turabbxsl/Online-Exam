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
