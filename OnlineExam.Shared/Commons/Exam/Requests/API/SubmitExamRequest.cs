using OnlineExam.Shared.Commons.Exam.Dtos.API;

namespace OnlineExam.Shared.Commons.Exam.Requests.API;

public record SubmitExamRequest(Guid ExamId, List<QuestionAnswerDto> Answers);
