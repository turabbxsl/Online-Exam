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

            await _hubContext.Clients.Group(msg.ExamId.ToString()).SendAsync("ReceiveExamStatus", new
            {
                Status = "Finished",
                Message = "İmtahan vaxtı bitdi!",
                Reason = msg.reason
            });
        }
    }
}
