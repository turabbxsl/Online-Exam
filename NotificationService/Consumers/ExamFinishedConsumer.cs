using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;
using OnlineExam.Shared.Contracts.Events;

namespace NotificationService.Consumers
{
    public class ExamFinishedConsumer : IConsumer<ExamFinishedEvent>
    {
        private readonly IHubContext<ExamHub> _hubContext;

        public ExamFinishedConsumer(IHubContext<ExamHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Consume(ConsumeContext<ExamFinishedEvent> context)
        {
            var msg = context.Message;

            string displayMessage = msg.reason == "TimeOut"
                ? "İmtahan vaxtı bitdi! Cavablarınız avtomatik qeydə alındı."
                : "İmtahanınız uğurla tamamlandı!";

            await _hubContext.Clients.Group(msg.ExamId.ToString()).SendAsync("ReceiveExamStatus", new
            {
                Status = "Finished",
                Message = displayMessage,
                Reason = msg.reason,
                ExamId = msg.ExamId
            });
        }
    }
}
