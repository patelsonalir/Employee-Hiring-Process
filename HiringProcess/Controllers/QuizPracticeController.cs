using HiringProcess.Data;
using HiringProcess.Models;
using HiringProcess.Models.View_Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HiringProcess.Controllers
{
    public class QuizPracticeController : Controller
    {
        public ApplicationDbContext _dbContext { get; set; }
        public QuizPracticeController(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }
        [HttpGet]

        public async Task<IActionResult> QuizList() 
        { 
            var quiz= await _dbContext.Quiz.ToListAsync();
            return View(quiz);
        }
        [HttpGet]

        public async Task<IActionResult> QuizPractice(int QuizId, string QuizName)
        {
            ViewBag.QuizId = QuizId;
            ViewBag.QuizName = QuizName;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> QuizQuestionAnswerList(int quizId)
        {
            var QuizQuestionAnswerList = _dbContext.QuizQuestionAnswer.Where(x => x.QuizID == quizId).ToList();
            var viewModel = QuizQuestionAnswerList
             .Select(item => new QuizQuestionAnswerVM
             {
                 numb = item.Id,
                 question = item.QuestionText,
                 answer = item.CorrectAnswer,
                 options = new List<string>
                   {
                       item.Option_A,
                       item.Option_B,
                       item.Option_C,
                       item.Option_D
                   }
             })
         .ToList();
            return Json(new { data = viewModel });

        }
    }
}
