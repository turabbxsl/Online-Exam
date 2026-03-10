using Exam.UI.Models;
using Microsoft.AspNetCore.Mvc;
using OnlineExam.Shared.Commons.Exam.Dtos.API;
using OnlineExam.Shared.Commons.Exam.Requests.API;
using OnlineExam.Shared.Commons.Exam.Responses.API;
using System.Text.Json;

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


        #region Gets


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

        [HttpGet]
        public async Task<IActionResult> Result(Guid examId)
        {
            var client = _httpClientFactory.CreateClient("ExamApi");

            try
            {
                var response = await client.GetAsync($"api/Exams/result/{examId}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "İmtahan nəticəsi tapılmadı.";
                    return RedirectToAction("Index");
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var resultData = await response.Content.ReadFromJsonAsync<ExamResultVM>(options);

                if (resultData == null) return RedirectToAction("Index");

                return View(resultData);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }


        #endregion

        #region Posts

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

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitExamRequest request)
        {
            var client = _httpClientFactory.CreateClient("ExamApi");

            var response = await client.PostAsJsonAsync("api/Exams/submit", request);

            if (response.IsSuccessStatusCode)
                return Ok(new { success = true });

            return BadRequest();
        }

        #endregion

    }
}
