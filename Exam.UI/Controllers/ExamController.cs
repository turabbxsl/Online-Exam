using Exam.UI.Models;
using Microsoft.AspNetCore.Mvc;
using OnlineExam.Shared.Commons;

namespace Exam.UI.Controllers
{
    public class ExamController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ExamController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index() => View();


        [HttpPost]
        public async Task<IActionResult> Start(string username)
        {
            var client = _httpClientFactory.CreateClient("ExamApi");

            var response = await client.PostAsJsonAsync("api/Exams/start", new { Username = username });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ExamStartResponse>();
                return RedirectToAction("Details", new { examId = result.ExamId });
            }

            TempData["ErrorMessage"] = "Sistemə giriş mümkün olmadı.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid examId)
        {
            var client = _httpClientFactory.CreateClient("ExamApi");

            var response = await client.GetAsync($"api/Exams/{examId}/questions");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return View("Waiting", examId);
            }

            if (response.IsSuccessStatusCode)
            {
                var examData = await response.Content.ReadFromJsonAsync<ExamDetailsDto>();

                var model = new ExamVM
                {
                    ExamId = examId,
                    Username = examData.Username,
                    Questions = examData.Questions
                };

                return View(model);
            }

            return View("Error");
        }


    }
}
