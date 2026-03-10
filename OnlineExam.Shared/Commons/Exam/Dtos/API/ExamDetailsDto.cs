namespace OnlineExam.Shared.Commons.Exam.Dtos.API
{
    public class ExamDetailsDto
    {
        public Guid ExamId { get; set; }
        public string Username { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }
}
