namespace OnlineExam.Shared.Contracts
{

    // Message that the system will send to itself when the timer expires
    public record ExamTimeoutExpired(Guid ExamId);
}
