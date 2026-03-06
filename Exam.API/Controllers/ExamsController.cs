using Dapper;
using Exam.API.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OnlineExam.Shared.Commons;
using OnlineExam.Shared.Contracts.Events;
using System.Data;

namespace Exam.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IDbConnection _dbConnection;

        public ExamsController(IPublishEndpoint publishEndpoint, IDbConnection dbConnection)
        {
            _publishEndpoint = publishEndpoint;
            _dbConnection = dbConnection;
        }


        [HttpPost("start")]
        public async Task<IActionResult> StartExam([FromBody] StartExamRequest request)
        {
            var sql = @"
        INSERT INTO Students (Username) 
        VALUES (@Username) 
        ON CONFLICT (Username) DO UPDATE SET Username = EXCLUDED.Username
        RETURNING Id;";

            var studentId = await _dbConnection.ExecuteScalarAsync<Guid>(sql, new { Username = request.UserName });

            var examId = NewId.NextGuid();

            await _publishEndpoint.Publish(new StartExam(examId, studentId));

            return Ok(new { ExamId = examId });
        }


        [HttpGet("{examId}/questions")]
        public async Task<IActionResult> GetQuestions(Guid examId)
        {
            var sql = @"
SELECT s.Username, q.Id, q.Content 
FROM public.""ExamStates"" es
JOIN public.""students"" s ON es.""StudentId"" = s.""id""
JOIN public.""examquestions"" eq ON es.""CorrelationId"" = eq.""examid""
JOIN public.""questions"" q ON eq.""questionid"" = q.""id""
WHERE es.""CorrelationId"" = @ExamId";

            var result = await _dbConnection.QueryAsync<dynamic>(sql, new { ExamId = examId });
            var dataList = result.ToList();

            if (!dataList.Any()) return NotFound();

            var response = new ExamDetailsDto
            {
                ExamId = examId,
                Username = dataList.First().username?.ToString() ?? "Naməlum",
                Questions = dataList.Select(r => new QuestionDto
                {
                    Id = r.id != null ? Convert.ToInt32(r.id) : 0,
                    Content = r.content?.ToString() ?? ""
                }).ToList()
            };

            return Ok(response);
        }


    }
}
