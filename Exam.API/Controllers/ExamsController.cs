using Dapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OnlineExam.Shared.Commons.Exam.Dtos.API;
using OnlineExam.Shared.Commons.Exam.Requests.API;
using OnlineExam.Shared.Commons.Exam.Responses.API;
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


        #region Gets

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


        [HttpGet("result/{examId}")]
        public async Task<IActionResult> GetExamResult(Guid examId)
        {
            var resultSql = @"SELECT score, 
                                     totalquestions, 
                                     correctanswers 
                              FROM public.examresults 
                              WHERE examid = @ExamId";

            var result = await _dbConnection.QuerySingleOrDefaultAsync<ExamResultDto>(resultSql, new { ExamId = examId });
            if (result == null)
                return NotFound("Bu imtahan üçün nəticə tapılmadı.");

            var detailsSql = @" SELECT  q.""content""               as QuestionText,
                                        qo_user.""OptionText""      as UserAnswer,
                                        qo_correct.""OptionText""   as CorrectAnswer,
                                        qo_user.""IsCorrect""       as IsCorrect
                               FROM public.studentanswers sa
                               JOIN public.""questions""       q          ON sa.questionId = q.""id""
                               JOIN public.""QuestionOptions"" qo_user    ON sa.selectedoptionId = qo_user.""Id""
                               JOIN public.""QuestionOptions"" qo_correct ON q.""id"" = qo_correct.""QuestionId"" AND qo_correct.""IsCorrect"" = TRUE
                               WHERE sa.examid = @ExamId";

            var details = await _dbConnection.QueryAsync<ExamResultDetailDto>(detailsSql, new { ExamId = examId });

            var response = new ExamResultResponse(
                Score: (result.Score),
                TotalQuestions: (int)result.TotalQuestions,
                CorrectAnswers: (int)result.CorrectAnswers,
                Details: details
            );

            return Ok(response);
        }


        [HttpGet("top-scores")]
        public async Task<IActionResult> GetTopScores()
        {
            var sql = @"SELECT s.username, 
                               MAX(r.score) as score
                        FROM public.examresults r
                        JOIN public.students s ON s.id = r.studentid
                        GROUP BY s.id, s.username
                        ORDER BY score DESC 
                        LIMIT 10";

            var topScores = await _dbConnection.QueryAsync<LeaderboardDto>(sql);
            return Ok(topScores);
        }

        #endregion

        #region Posts

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


        [HttpPost("submit")]
        public async Task<IActionResult> SubmitForm([FromBody] SubmitExamRequest request)
        {
            var insertSql = @"INSERT INTO public.studentanswers (ExamId, QuestionId, SelectedOptionId) 
                      VALUES (@ExamId, @QuestionId, @SelectedOptionId)";

            await _dbConnection.ExecuteAsync(insertSql,
                request.Answers.Select(x => new { request.ExamId, x.QuestionId, x.SelectedOptionId }));

            var scoreSql = @"SELECT COUNT(*) as Total,
                            SUM(CASE WHEN qo.""IsCorrect"" = TRUE THEN 1 ELSE 0 END) as Correct
                     FROM public.studentanswers sa
                     JOIN public.""QuestionOptions"" qo ON sa.selectedoptionId = qo.""Id""
                     WHERE sa.ExamId = @ExamId";

            var scoreData = await _dbConnection.QuerySingleAsync<dynamic>(scoreSql, new { ExamId = request.ExamId });

            int total = (int)scoreData.total;
            int correct = Convert.ToInt32(scoreData.correct ?? 0);
            decimal score = total > 0 ? (decimal)correct / total * 100 : 0;

            var studentSql = @"SELECT ""StudentId"" FROM public.""ExamStates"" WHERE ""CorrelationId"" = @ExamId";
            var studentId = await _dbConnection.QuerySingleAsync<Guid>(studentSql, new { ExamId = request.ExamId });

            var resultSql = @"INSERT INTO public.examresults (ExamId, StudentId, TotalQuestions, CorrectAnswers, Score)
                      VALUES (@ExamId, @StudentId, @Total, @Correct, @Score)";

            await _dbConnection.ExecuteAsync(resultSql,
                new { request.ExamId, StudentId = studentId, Total = total, Correct = correct, Score = score });

            var submittedAnswersForSaga = request.Answers
                .Select(x => new SubmittedAnswerDto(x.QuestionId, x.SelectedOptionId))
                .ToList();

            await _publishEndpoint.Publish(new SubmitAnswers(
                request.ExamId,
                studentId,
                submittedAnswersForSaga
            ));

            return Ok(new { Message = "İmtahan uğurla tamamlandı", Score = score });
        }

        #endregion

    }
}
