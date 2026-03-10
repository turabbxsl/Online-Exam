using Microsoft.AspNetCore.Mvc;
using OnlineExam.Shared.Commons.Exam.Dtos.API;

namespace Exam.UI.ViewComponents
{
    public class LeaderboardViewComponent : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public LeaderboardViewComponent(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient("ExamApi");
            var topScores = await client.GetFromJsonAsync<List<LeaderboardDto>>("api/Exams/top-scores");
            return View(topScores ?? new List<LeaderboardDto>());
        }
    }
}
