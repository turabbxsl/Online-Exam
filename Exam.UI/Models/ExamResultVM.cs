namespace Exam.UI.Models
{
    public class ExamResultVM
    {
        public decimal Score { get; init; }
        public int TotalQuestions { get; init; }
        public int CorrectAnswers { get; init; }
        public List<ExamDetailVM> Details { get; init; } = new();
    }

    public class ExamDetailVM
    {
        public string QuestionText { get; init; }
        public string UserAnswer { get; init; }
        public string CorrectAnswer { get; init; }
        public bool IsCorrect { get; init; }
    }
}
