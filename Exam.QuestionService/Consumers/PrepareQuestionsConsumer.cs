using Dapper;
using MassTransit;
using OnlineExam.Shared.Contracts.Commands;
using OnlineExam.Shared.Contracts.Events;
using System.Data;

namespace Exam.QuestionService.Consumers
{
    public class PrepareQuestionsConsumer : IConsumer<PrepareQuestions>
    {

        private readonly ILogger<PrepareQuestionsConsumer> _logger;
        private readonly IDbConnection _dbConnection;

        public PrepareQuestionsConsumer(ILogger<PrepareQuestionsConsumer> logger, IDbConnection dbConnection)
        {
            _logger = logger;
            _dbConnection = dbConnection;
        }

        public async Task Consume(ConsumeContext<PrepareQuestions> context)
        {
            var examId = context.Message.ExamId;

            var sqlSelect = "SELECT Id FROM QUESTIONS ORDER BY RANDOM() LIMIT 5";
            var questionIds = (await _dbConnection.QueryAsync<int>(sqlSelect)).ToList();

            if (!questionIds.Any())
            {
                _logger.LogWarning("sual tapılmadı!");
                return;
            }

            var sqlInsert = "INSERT INTO ExamQuestions (ExamId, QuestionId) VALUES (@ExamId, @QuestionId)";
            foreach (var qId in questionIds)
                await _dbConnection.ExecuteAsync(sqlInsert, new { ExamId = examId, QuestionId = qId });

            await context.Publish(new QuestionPrepared(examId));

            _logger.LogInformation("İmtahan {ExamId} üçün {Count} sual hazırlandı.", examId, questionIds.Count);
        }
    }
}
