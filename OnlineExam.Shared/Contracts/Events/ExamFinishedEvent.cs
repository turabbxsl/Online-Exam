namespace OnlineExam.Shared.Contracts.Events
{
    public record ExamFinishedEvent(Guid ExamId, Guid StudentId, string reason);
}
