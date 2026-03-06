namespace OnlineExam.Shared.Contracts.Events
{

    // The event that started the saga
    public record StartExam(Guid ExamId, Guid StudentId);
}
