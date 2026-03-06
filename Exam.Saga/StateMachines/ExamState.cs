using MassTransit;

namespace Exam.Saga.StateMachines
{
    public class ExamState : SagaStateMachineInstance
    {
        // Unique ID (Primary Key) used by MassTransit
        public Guid CorrelationId { get; set; }

        // Current status of the exam (e.g. Preparing, InProgress, Finished)
        // Will be stored as a string in SQL
        public string CurrentState { get; set; }

        public Guid StudentId { get; set; }
        public DateTime? StartTime { get; set; }

        // The ticket number needed to control the timer (60 minutes).
        // MassTransit will fill and empty this automatically.
        public Guid? ExpirationTokenId { get; set; }
    }
}
