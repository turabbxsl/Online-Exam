namespace OnlineExam.Shared.Commons.Exam.Dtos.API
{
    public class QuestionDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public List<OptionDto> Options { get; set; } = new();
    }

    public class OptionDto
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
    }
}
