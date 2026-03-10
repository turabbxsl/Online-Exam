namespace OnlineExam.Shared.Commons.Exam.Dtos.API
{
    public record ExamResultDetailDto(string QuestionText, string UserAnswer, string CorrectAnswer, bool IsCorrect);
}
