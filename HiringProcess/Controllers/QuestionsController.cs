using HiringProcess.Data;
using HiringProcess.Models;
using HiringProcess.Models.View_Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HiringProcess.Controllers
{
    public class QuestionsController : Controller
    {
        public ApplicationDbContext _dbContext { get; set; }
        public QuestionsController(ApplicationDbContext applicationDbContext)
        {
            this._dbContext = applicationDbContext;
        }
        public IActionResult QuestionsEntry()
        {

            var Quiz = _dbContext.Quiz.Where(x => x.IsActive == true).ToList();
            ViewData["QuizData"] = Quiz;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuestionsEntry(QuizQuestionsAnswerEntryVM model)
        {
      
                foreach (var Question in model.QuizQuestionAnswers)
                {
                    //if (!string.IsNullOrEmpty(Question))
                    //{
                        var questions = new QuizQuestionAnswer
                        {
                            QuestionText = Question.QuestionText,
                            Option_A=Question.Option_A,
                            Option_B=Question.Option_B,
                            Option_C=Question.Option_C,
                            Option_D=Question.Option_D,
                            CorrectAnswer= Question.CorrectAnswer
                            //Category = category
                        };
                    _dbContext.QuizQuestionAnswer.Add(questions);
                //}
            }
            await _dbContext.SaveChangesAsync();

            var Quiz = _dbContext.Quiz.Where(x => x.IsActive == true).ToList();
                ViewData["QuizData"] = Quiz;
                return RedirectToAction(nameof(Index));  // Redirect to the Index page or another action
            }

        }
    }

