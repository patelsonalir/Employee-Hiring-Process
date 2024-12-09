using HiringProcess.Data;
using HiringProcess.Interface;
using HiringProcess.Models;
using HiringProcess.Models.View_Model;
using Microsoft.AspNetCore.Mvc;
using Twilio.Types;
using Twilio;
using Microsoft.EntityFrameworkCore;

namespace HiringProcess.Controllers
{
    public class CandidateMarksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICandidateInterview _candidateInterview;
        private readonly ICandidate _candidate;

        public CandidateMarksController(ApplicationDbContext context, ICandidateInterview candidateInterview, ICandidate candidate)
        {
            _context = context;
            _candidateInterview = candidateInterview;
            _candidate = candidate;
        }

        // Schedule Interview
        public async Task<IActionResult> CandidateInterviewMarksList()
        {

            var model = await _candidateInterview.CandidateInterviewMarksGetList();
            return View(model);
        }
        public async Task<IActionResult> CandidateInterviewMarksNewAdd()
        {

            // Fetch data for candidates and interview rounds
            var resultCandidate = await _candidateInterview.GetCandidateList();
            ViewData["CandidateData"] = resultCandidate;

            var resultInterviewRound = await _candidateInterview.GetInterviewRoundWithQuizList();
            ViewData["InterviewRoundData"] = resultInterviewRound;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CandidateInterviewMarksNewSave(CandidateInterviewRoundResult model)
        {
            // Check if the candidate already has interview marks for this round
            var existingInterviewMark = await _candidateInterview.IsCandidateAlreadyInterviewMarks(model.CandidateId, model.InterviewRoundId);

            if (existingInterviewMark != null)
            {
                TempData["ErrorMessage"] = "Candidate already has interview marks for this round.";
                return RedirectToAction(nameof(CandidateInterviewMarksList));
            }

            // Retrieve the interview round by Id
            var interviewRound = await _candidateInterview.GetInterviewRoundById(model.InterviewRoundId);

            if (interviewRound == null)
            {
                TempData["ErrorMessage"] = "Invalid interview round.";
                return RedirectToAction(nameof(CandidateInterviewMarksList));
            }

            var CandidateInterviewScheduled = await _candidateInterview.CandidateInterviewScheduledGetListById(model.CandidateId, model.InterviewRoundId);

            if (CandidateInterviewScheduled == null)
            {
                TempData["ErrorMessage"] = "Candidate does not have a valid scheduled or rescheduled interview for this round.";
                return RedirectToAction(nameof(CandidateInterviewMarksList));
            }

            CandidateInterviewRoundResult candidateResult = new CandidateInterviewRoundResult
            {
                Id = model.Id,
                CandidateId = model.CandidateId,
                InterviewRoundId = model.InterviewRoundId,
                CandidateOutOfScore = model.CandidateOutOfScore,
                CandidateScore = model.CandidateScore,
                InterviewDate = model.InterviewDate,
                IsEligible = model.CandidateOutOfScore > 0 && (double)model.CandidateScore / model.CandidateOutOfScore * 100 >= 60
            };

            await _candidateInterview.CandidateInterviewMarksSave(candidateResult);

            return RedirectToAction(nameof(CandidateInterviewMarksList));
        }
        public async Task<IActionResult> CandidateInterviewMarksEdit(int Id)
        {
            CandidateInterviewVm model = new CandidateInterviewVm();
            model = await _candidateInterview.GetCandidateInterviewMarks(Id);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ScheduleInterviewEditSave(CandidateInterviewVm model)
        {

            CandidateInterviewRoundResult candidateResult = new CandidateInterviewRoundResult
            {
                Id = model.CandidateInterviewRoundResult.Id,
                CandidateId = model.CandidateInterviewRoundResult.CandidateId,
                InterviewRoundId = model.CandidateInterviewRoundResult.InterviewRoundId,
                CandidateOutOfScore = model.CandidateInterviewRoundResult.CandidateOutOfScore,
                CandidateScore = model.CandidateInterviewRoundResult.CandidateScore,
                InterviewDate = model.CandidateInterviewRoundResult.InterviewDate,
                IsEligible = model.CandidateInterviewRoundResult.CandidateOutOfScore > 0 && (double)model.CandidateInterviewRoundResult.CandidateScore / model.CandidateInterviewRoundResult.CandidateOutOfScore * 100 >= 60
            };

            await _candidateInterview.CandidateInterviewMarksSave(candidateResult);



            return RedirectToAction(nameof(CandidateInterviewMarksList));
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCandidateInterviewRoundMarks(int Id)
        {

            await _candidateInterview.CandidateInterviewMarksDelete(Id);

            return RedirectToAction(nameof(CandidateInterviewMarksList));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAllCandidateInterviewsMakrs(int CandidateId)
        {
            try
            {
                bool isDeleted = await _candidateInterview.CandidateAllInterviewMarksDelete(CandidateId);

                if (isDeleted)
                {
                    return RedirectToAction(nameof(CandidateInterviewMarksList));
                }
                else
                {
                    return StatusCode(500, "An error occurred while deleting the interviews.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpGet]
        public JsonResult ViewAllInterviewsMarks(int candidateId)
        {
            var candidate = _candidateInterview.GetCandidateWithInterviewsMarks(candidateId);

            return Json(candidate);
        }
        [HttpGet]
        public JsonResult GetInterviewsDetailsMarksById(int Id)
        {
            var model = _candidateInterview.GetCandidateInterviewMarksById(Id);

            return Json(model);
        }

        // Action to get the interview marks for editing
        [HttpGet]
        public async Task<IActionResult> CandidateInterviewMarksEditAll(int CandidateId)
        {
            var candidateInterviewMarksVm = _candidateInterview.GetInterviewMarksByCandidateId(CandidateId);

            if (candidateInterviewMarksVm == null)
            {
                return await Task.FromResult(NotFound());  // Ensure NotFound is wrapped as a Task
            }

            return View(candidateInterviewMarksVm);  // Pass data to the view
        }


        [HttpPost]
        public async Task<IActionResult> CandidateInterviewMarksEditAll(CandidateInterviewVm model)
        {
                foreach (var round in model.CandidateInterviewRounds)
                {
                    // Find the corresponding record in CandidateInterviewRoundResult using CandidateId and RoundId
                    var roundResult = await _candidateInterview.IsCandidateAlreadyInterviewMarks(round.CandidateId, round.InterviewRoundId);
                 
                    if (roundResult != null)
                    {
                        roundResult.Id = round.Id;
                        roundResult.CandidateId = round.CandidateId;
                        roundResult.CandidateScore = round.CandidateScore;
                        roundResult.CandidateOutOfScore = round.CandidateOutOfScore;
                        roundResult.InterviewDate = round.InterviewDate;
                        roundResult.IsEligible = round.CandidateOutOfScore > 0 && ((double)round.CandidateScore / (double)round.CandidateOutOfScore) * 100 >= 60;
                    }

                    await _candidateInterview.CandidateInterviewMarksSave(roundResult);
                }


                // Redirect to the list of marks or any other appropriate view
                return RedirectToAction("CandidateInterviewMarksList", "CandidateMarks");

            // If the model is not valid, re-render the page with validation errors
            return View(model);
        }

    }
}
