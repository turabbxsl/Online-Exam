namespace OnlineExam.Shared.Contracts.Events
{

    // Student's responses (Command)
    public record SubmitAnswers(Guid ExamId, string AnswersJson);
}
