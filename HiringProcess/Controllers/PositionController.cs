using HiringProcess.Data;
using HiringProcess.Models;
using Microsoft.AspNetCore.Mvc;

namespace HiringProcess.Controllers
{
    public class PositionController : Controller
    {
        public ApplicationDbContext _dbContext { get; set; }
        public PositionController(ApplicationDbContext applicationDbContext)
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
            var Position = _dbContext.Position.ToList();

            if (Position != null)
            {
                return Json(Position);
            }
            return Json(new { success = false });
        }
        [HttpGet]
        public async Task<IActionResult> GetById(int Id)
        {

            var Position = _dbContext.Position.Where(x=>x.Id == Id).FirstOrDefault();
            if (Position != null)
            {
                return Json(new { data = Position });
            }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> SavePosition([FromBody] Position Position)
        {

            string JsonResultMessage = "";
            try
            {

                    if (JsonResultMessage == "")
                    {

                        if (Position.Id == 0)
                              _dbContext.Position.Add(Position);

                        else
                            _dbContext.Position.Update(Position);

                        _dbContext.SaveChanges();
                        JsonResultMessage = "Success " + Position.Name + ", Saved Successfully.";
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
        public async Task<IActionResult> DeletePosition(int Id)
        {

            string JsonResultMessage = "";
            try
            {
                bool DeletedPosition = false;
                var Position = _dbContext.Position.Find(Id);
                if (Position != null)
                {
                    _dbContext.Position.Remove(Position);
                    _dbContext.SaveChanges();
                    DeletedPosition = true;
                }
                else { DeletedPosition = false; }

                if (DeletedPosition == true)
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

