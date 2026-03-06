namespace OnlineExam.Shared.Commons
{
    public class ExamDetailsDto
    {
        public Guid ExamId { get; set; }
        public string Username { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }
}
