using HiringProcess.Data;
using HiringProcess.Models;
using Microsoft.AspNetCore.Mvc;

namespace HiringProcess.Controllers
{
    public class QuizController : Controller
    {
        public ApplicationDbContext _dbContext { get; set; }
        public QuizController(ApplicationDbContext applicationDbContext)
        {
            this._dbContext = applicationDbContext;
        }
        public IActionResult Index()
        {
     
            var Position = _dbContext.Position.Where(x=>x.IsActive == true).ToList();
            ViewData["PositionData"] = Position;
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = (from c in _dbContext.Quiz
                         join s in _dbContext.Position on c.PositionId equals s.Id

                         select new Quiz
                         {
                             Id = c.Id,
                             Name = c.Name,
                             PositionId = c.PositionId,
                             PositionName = s.Name,
                             IsActive = c.IsActive,
                         });

            var Quiz = query.ToList();

            if (Quiz != null)
            {
                return Json(Quiz);
            }
            return Json(new { success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {

            var Quiz = _dbContext.Quiz.Where(x => x.Id == Id).FirstOrDefault();
            if (Quiz != null)
            {
                return Json(new { data = Quiz });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> SaveQuiz([FromBody] Quiz Quiz)
        {

            string JsonResultMessage = "";
            try
            {

                 if (JsonResultMessage == "")
                    {

                        if (Quiz.Id == 0)
                            _dbContext.Quiz.Add(Quiz);

                        else
                            _dbContext.Quiz.Update(Quiz);

                        _dbContext.SaveChanges();
                        JsonResultMessage = "Success " + Quiz.Name + ", Saved Successfully.";
                        return Json(new { data = JsonResultMessage });
                    }
                

            }
            catch (Exception ex)
            {
                JsonResultMessage = "Error " + JsonResultMessage + " " + ex.Message;

            }
            return Json(new { data = JsonResultMessage });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteQuiz(int Id)
        {

            string JsonResultMessage = "";
            try
            {
                bool DeletedQuiz = false;
                var Quiz = _dbContext.Quiz.Find(Id);
                if (Quiz != null)
                {
                    _dbContext.Quiz.Remove(Quiz);
                    _dbContext.SaveChanges();
                    DeletedQuiz = true;
                }
                else { DeletedQuiz = false; }

                if (DeletedQuiz == true)
                    JsonResultMessage = "Success Deleted Successfully.";
                else
                    JsonResultMessage = "Error on Deleted unsuccessfully.";

            }
            catch (Exception ex)
            {
                JsonResultMessage = "Error " + JsonResultMessage + " " + ex.Message;

            }
            return Json(new { data = JsonResultMessage });
        }
    }
}
