using OnlineExam.Shared.Commons.Exam.Dtos.API;

namespace OnlineExam.Shared.Contracts.Events
{

    // Student's responses (Command)
    public record SubmitAnswers(Guid ExamId, Guid StudentId, List<SubmittedAnswerDto> Answers);
}

