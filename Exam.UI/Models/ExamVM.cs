using OnlineExam.Shared.Commons;

namespace Exam.UI.Models
{
        public class ExamVM
        {
            public Guid ExamId { get; set; }
            public string Username { get; set; } 
            public List<QuestionDto> Questions { get; set; }
        }
}
