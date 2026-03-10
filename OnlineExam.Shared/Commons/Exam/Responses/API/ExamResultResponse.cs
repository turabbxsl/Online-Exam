using OnlineExam.Shared.Commons.Exam.Dtos.API;

namespace OnlineExam.Shared.Commons.Exam.Responses.API
{
    public record ExamResultResponse(
    decimal Score,
    int TotalQuestions,
    int CorrectAnswers,
    IEnumerable<ExamResultDetailDto> Details
    );
}
