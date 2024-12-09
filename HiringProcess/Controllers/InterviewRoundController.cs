using HiringProcess.Data;
using HiringProcess.Models;
using Microsoft.AspNetCore.Mvc;

namespace HiringProcess.Controllers
{
    public class InterviewRoundController : Controller
    {
        public ApplicationDbContext _dbContext { get; set; }
        public InterviewRoundController(ApplicationDbContext applicationDbContext)
        {
            this._dbContext = applicationDbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try{
                var InterviewRound = _dbContext.InterviewRound.ToList();

                if (InterviewRound != null)
                {
                    return Json(InterviewRound);
                }
            }
            catch (Exception ex) {
                ex.Message.ToString();  
            }
            return Json(new { success = false });

        }
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {

            var InterviewRound = _dbContext.InterviewRound.Where(x => x.Id == Id).FirstOrDefault();
            if (InterviewRound != null)
            {
                return Json(new { data = InterviewRound });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> SaveInterviewRound([FromBody] InterviewRound InterviewRound)
        {

            string JsonResultMessage = "";
            try
            {

                if (ModelState.IsValid)
                {
                    if (JsonResultMessage == "")
                    {

                        if (InterviewRound.Id == 0)
                            _dbContext.InterviewRound.Add(InterviewRound);

                        else
                            _dbContext.InterviewRound.Update(InterviewRound);

                        _dbContext.SaveChanges();
                        JsonResultMessage = "Success " + InterviewRound.RoundName + ", Saved Successfully.";
                        return Json(new { data = JsonResultMessage });
                    }
                }

            }
            catch (Exception ex)
            {
                JsonResultMessage = "Error " + JsonResultMessage + " " + ex.Message;

            }
            return Json(new { data = JsonResultMessage });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteInterviewRound(int Id)
        {

            string JsonResultMessage = "";
            try
            {
                bool DeletedInterviewRound = false;
                var InterviewRound = _dbContext.InterviewRound.Find(Id);
                if (InterviewRound != null)
                {
                    _dbContext.InterviewRound.Remove(InterviewRound);
                    _dbContext.SaveChanges();
                    DeletedInterviewRound = true;
                }
                else { DeletedInterviewRound = false; }

                if (DeletedInterviewRound == true)
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
