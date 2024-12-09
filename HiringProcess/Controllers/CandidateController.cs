using HiringProcess.Data;
using HiringProcess.Interface;
using HiringProcess.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.EntityFrameworkCore;
using System;
using static Azure.Core.HttpHeader;

namespace HiringProcess.Controllers
{
    public class CandidateController : Controller
    {
        public readonly ICandidate _Candidate;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CandidateController(ICandidate Candidate, IWebHostEnvironment webHostEnvironment)
        {
            this._Candidate = Candidate;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> CandidateList()
        {

            //var resultInterviewRound = await _Candidate.GetInterviewRoundList();
         //   ViewData["InterviewRoundData"] = resultInterviewRound;

            var Candidate = await _Candidate.CandidateGetList();

            //ViewBag.CandidateCount = await _Candidate.CandidateCount();

            //ViewBag.StatusList = Enum.GetValues(typeof(Status)).Cast<Status>().Select(m => new SelectListItem
            //{
            //    Text = m.ToString(),
            //    Value = ((int)m).ToString()
            //}).ToList(); 
            //ViewBag.RejectedReasonList = Enum.GetValues(typeof(RejectedReason)).Cast<RejectedReason>().Select(m => new SelectListItem
            //{
            //    Text = m.ToString(),
            //    Value = ((int)m).ToString()
            //}).ToList();

            return View(Candidate);
        }
        public async Task<IActionResult> CandidateEntry(int CandidateId)
        {
            var resultPosition = await _Candidate.GetPositionList();
            ViewData["PositionData"] = resultPosition;

            Candidate model = new Candidate();
            if (CandidateId == 0)
            {
                model.ProfilePic = "Candidatedefault.png";
                model.Status = "New Candidate";
            }
            else
            {

                model = await _Candidate.GetCandidate(CandidateId);
               
            }
            //model.StatusList = Enum.GetValues(typeof(Status)).Cast<Status>().Select(m => new SelectListItem
            //{
            //    Text = m.ToString(),
            //    Value = ((int)m).ToString()
            //}).ToList();
            //model.RejectedReasonList = Enum.GetValues(typeof(RejectedReason)).Cast<RejectedReason>().Select(m => new SelectListItem
            //{
            //    Text = m.ToString(),
            //    Value = ((int)m).ToString()
            //}).ToList();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CandidateEntry(Candidate model)
        {
          
            Candidate Candidate = new Candidate();
            string uniqueFileName = UploadedFile(model, 1);
            Candidate = await _Candidate.GetCandidate(model.Id);

            if (string.IsNullOrEmpty(uniqueFileName))
            {

                if (model.Id != 0)
                {
                    if (string.IsNullOrEmpty(Candidate.ProfilePic))
                    {
                        model.ProfilePic = "Candidatedefault.png";
                    }
                    else
                    {
                        uniqueFileName = Candidate.ProfilePic;
                    }
                    if (model.ProfilePic == uniqueFileName)
                    {
                        uniqueFileName = model.ProfilePic;
                    }
                }
                else
                {
                    uniqueFileName = "Candidatedefault.png";
                }
            }

            string ResumeileName = "";
            string ResumeileNameContentType = "";

            if (model.Id != 0)
            {
                if (model.ResumeIform != null)
                {
                    ResumeileName = UploadedFile(model, 2);
                    ResumeileNameContentType = model.ResumeIform.ContentType;
                }
                else
                {
                    ResumeileName = Candidate.ResumePath;
                    ResumeileNameContentType = Candidate.ResumeContentType;
                }
            }
            else {
                if (model.ResumeIform != null)
                {
                    ResumeileName = UploadedFile(model, 2);
                    ResumeileNameContentType = model.ResumeIform.ContentType;
                }
                else
                {
                    ResumeileName = "";
                    ResumeileNameContentType = "";
                }
            }
      
            Candidate candidate = new Candidate
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                ProfilePic = uniqueFileName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PositionId = model.PositionId,
                ExperinceMonth = model.ExperinceMonth,
                ExperinceYear = model.ExperinceYear,
                Id = model.Id,
                CandidateName = model.FirstName + "_" + model.LastName,
                ResumeContentType = ResumeileNameContentType,
                ResumePath = ResumeileName,
                Status = model.Status,
                RejectedReason = model.RejectedReason,
            };
           Candidate = await _Candidate.CandidateSave(candidate);


            return RedirectToAction(nameof(CandidateList));

        }
        private string UploadedFile(Candidate model, int mod)
        {
            string uniqueFileName = null;

            if (mod == 1)
            {
                if (model.PicturePathIform != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Candidate/ProfilePictures");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.PicturePathIform.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.PicturePathIform.CopyTo(fileStream);
                    }
                }
            }
            else if (mod == 2)
            {
                if (model.ResumeIform != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Candidate/Remuse");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ResumeIform.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ResumeIform.CopyTo(fileStream);
                    }
                }
            }

            return uniqueFileName;
        }
        [HttpPost]

        public IActionResult GetDocument(string FileName, string ContentType)
        {
            string filePath = Path.Combine("/Candidate/Remuse", FileName);

            var response = new FileResponse
            {
                fileContentType = ContentType,
                filePath = filePath,
            };
            return Json(response);


        }
        [HttpPost]
        public async Task<IActionResult> DeleteCandidateDetails(int CandidateId)
        {

            _Candidate.CandidateDelete(CandidateId);

            return RedirectToAction(nameof(CandidateList));
        }
        //[HttpPost]
        //public async Task<IActionResult> CandidateInterviewRoundChange(int CandidateId, int InterviewRound, int Marks,int MarksOutOf,string InterviewDate)
        //{
        //    CandidateInterviewRoundResult candidateInterviewRoundResult=new CandidateInterviewRoundResult();
            
        //    candidateInterviewRoundResult.Id = CandidateId;
        //    candidateInterviewRoundResult.InterviewRoundId = InterviewRound;
        //    candidateInterviewRoundResult.CandidateOutOfScore = MarksOutOf;
        //    candidateInterviewRoundResult.CandidateScore = MarksOutOf;
        //    candidateInterviewRoundResult.IsEligible = MarksOutOf >= (MarksOutOf * 0.6);
        //    candidateInterviewRoundResult.InterviewDate = Convert.ToDateTime(InterviewDate);



        //  await  _Candidate.CandidateInterviewRoundChange(candidateInterviewRoundResult);

        //    return RedirectToAction(nameof(CandidateList));
        //}
        //[HttpPost]
        //public async Task<IActionResult> CandidateInterviewScheduled(int ScheduledId,int ReScheduledId,int CandidateId, int InterviewRound,string AlreadyScheduledDate,bool IsReScheduledDate, string ScheduledDate)
        //{
        //    CandidateInterviewRoundScheduled interviewRoundScheduled = new CandidateInterviewRoundScheduled();
        //    interviewRoundScheduled.Id = ScheduledId;
        //    if (string.IsNullOrEmpty(AlreadyScheduledDate))
        //    {

        //        interviewRoundScheduled.ScheduledDate = Convert.ToDateTime(ScheduledDate);
        //    }
        //    else
        //    {
        //        interviewRoundScheduled.ScheduledDate = Convert.ToDateTime(AlreadyScheduledDate);
        //        CandidateReScheduledInterview candidateReScheduledInterview = new CandidateReScheduledInterview();  
        //        candidateReScheduledInterview.Id = ReScheduledId;
        //        candidateReScheduledInterview.CandidateId = CandidateId;
        //        candidateReScheduledInterview.ReScheduledDate= Convert.ToDateTime(ScheduledDate);   
        //        candidateReScheduledInterview.RoundId= InterviewRound;
        //        await _Candidate.CandidateReScheduledInterviewdSave(candidateReScheduledInterview);

        //    }
        //    interviewRoundScheduled.RoundId = InterviewRound;
        //    interviewRoundScheduled.CandidateId = CandidateId;
        //    interviewRoundScheduled.IsReScheduledDate = false;

        //   await _Candidate.CandidateInterviewRoundScheduledSave(interviewRoundScheduled);

        //    return RedirectToAction(nameof(CandidateList));

        //}
    }
}
