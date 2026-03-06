using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs
{
    public class ExamHub : Hub
    {
        public async Task JoinToExamGroup(string examId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, examId);
        }
    }
}
