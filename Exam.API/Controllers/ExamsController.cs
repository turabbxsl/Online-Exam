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
            var sql = @"SELECT s.""username"", 
                               q.""id"" as QuestionId, 
	                           q.""content"", 
                               qo.""Id"" as OptionId, 
	                           qo.""OptionText""
                        FROM public.""ExamStates"" es
                        JOIN public.Students s ON es.""StudentId"" = s.""id""
                        JOIN public.examquestions eq ON es.""CorrelationId"" = eq.""examid""
                        JOIN public.Questions q ON eq.""questionid"" = q.""id""
                        LEFT JOIN public.""QuestionOptions"" qo ON q.""id"" = qo.""QuestionId""
                        WHERE es.""CorrelationId"" = @ExamId";

            var questionDict = new Dictionary<int, QuestionDto>();
            string userName = "";

            var result = await _dbConnection.QueryAsync<dynamic, dynamic, QuestionDto>(
                sql,
                (q, qo) =>
                {
                    if (string.IsNullOrEmpty(userName)) userName = q.username;

                    if (!questionDict.TryGetValue((int)q.questionid, out var questionDto))
                    {
                        questionDto = new QuestionDto
                        {
                            Id = (int)q.questionid,
                            Content = (string)q.content
                        };
                        questionDict.Add(questionDto.Id, questionDto);
                    }

                    if (qo != null)
                    {
                        questionDto.Options.Add(new OptionDto
                        {
                            Id = (int)qo.optionid,
                            OptionText = (string)qo.OptionText
                        });
                    }
                    return questionDto;
                },
                    new { ExamId = examId },
                    splitOn: "OptionId"
                );

            if (!questionDict.Any()) return NotFound();

            return Ok(new ExamDetailsDto
            {
                ExamId = examId,
                Username = userName,
                Questions = questionDict.Values.ToList()
            });
        }


    }
}
