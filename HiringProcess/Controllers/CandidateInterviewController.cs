using HiringProcess.Data;
using HiringProcess.Interface;
using HiringProcess.Models;
using HiringProcess.Models.View_Model;
using Microsoft.AspNetCore.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HiringProcess.Controllers
{
    public class CandidateInterviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly ICandidateInterview _candidateInterview;
        private readonly ICandidate _candidate;

        public CandidateInterviewController(ApplicationDbContext context, IEmailSender emailSender, ICandidateInterview candidateInterview, ICandidate candidate)
        {
            _context = context;
            _emailService = emailSender;
            _candidateInterview = candidateInterview;
            _candidate = candidate;
        }

        // Schedule Interview
        public async Task<IActionResult> ScheduleInterviewList()
        {

            var model = await _candidateInterview.CandidateInterviewScheduledGetList();
            return View(model);
        }

        public async Task<IActionResult> ScheduleInterviewNewAdd()
        {

            // Fetch data for candidates and interview rounds
            var resultCandidate = await _candidateInterview.GetCandidateList();
            ViewData["CandidateData"] = resultCandidate;

            var resultInterviewRound = await _candidateInterview.GetInterviewRoundList();
            ViewData["InterviewRoundData"] = resultInterviewRound;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ScheduleInterviewNewSave(CandidateInterviewRoundScheduled model)
        {
            var existingInterview = await _candidateInterview.IsCandidateAlreadyInterviewScheduled(model.CandidateId,model.RoundId);

            if (existingInterview != null)
            {
                // If an interview already exists, show a message (using TempData or ViewBag)
                TempData["ErrorMessage"] = "Candidate already have an interview scheduled for this round.";
                return RedirectToAction(nameof(ScheduleInterviewList));  // Redirect to the interview list or relevant page
            }
            var interviewRound = await _candidateInterview.GetInterviewRoundById(model.RoundId);
            if (interviewRound == null)
            {
                // Handle case where the round name could not be found (optional)
                TempData["ErrorMessage"] = "Invalid interview round.";
                return RedirectToAction(nameof(ScheduleInterviewList));
            }
            string link = "";
           
            if (model.RoundId == 1)
            {
                link = "https://localhost:7289/CandidateSide/CandidateLogin";
            }
            else if (model.RoundId == 2 || model.RoundId == 4)
            {
                link = GenerateGoogleMeetLink();
            }
            else 
            {
                link = "";
            }
            CandidateInterviewRoundScheduled candidateIRS = new CandidateInterviewRoundScheduled
            {
                Id = model.Id,
                CandidateId = model.CandidateId,
                IsCompleted = false,
                IsRescheduled =  false,
                RoundId = model.RoundId,
                ScheduledDate = model.ScheduledDate,
                GoogleMeetLink = link,    
            };

            await _candidateInterview.CandidateInterviewScheduledSave(candidateIRS);
            var candidate = await _candidate.GetCandidate(model.CandidateId);

            //if (!string.IsNullOrEmpty(candidateEmail))
            //{
            //    string subject = "Your Interview is Scheduled";


            //    // Send the interview schedule email with the Google Meet link
            //    await SendInterviewScheduleEmail(subject, candidateEmail, googleMeetLink);
            //}
            if (!string.IsNullOrEmpty(candidate.PhoneNumber))
            {
                string roundName = interviewRound.RoundName;  // Assuming 'Name' is the field that holds the round name
                string rescheduleLink = Url.Action("ScheduleInterviewEdit", "CandidateInterview", new { Id = model.Id }, Request.Scheme);

                string messagelink = "";

                if (model.RoundId == 1)
                {
                    messagelink = " Please join the quiz interview using the following link given quiz test: https://localhost:7289/CandidateSide/CandidateLogin" +" . Quiz Username: " + candidate.CandidateName + " ." ;
                }
                else if (model.RoundId == 2 || model.RoundId == 4)
                {
                    messagelink = " Please join the interview using the following Google Meet link: " + link + " .";
                }
                else
                {
                    messagelink = "";
                }

                string message = $"Hello {candidate.FirstName} {candidate.LastName}, your {roundName} interview has been scheduled on {model.ScheduledDate}.{messagelink} If you need to reschedule, click here: {rescheduleLink}.";

                // Send a text message with the Google Meet link
           //     await SendTextMessage(candidate.PhoneNumber, message);
            }
            return RedirectToAction(nameof(ScheduleInterviewList));
        }

        private async Task SendTextMessage(string phoneNumber, string message)
        {
            // Initialize the Twilio client with your Account SID and Auth Token
            TwilioClient.Init("ACdc5a6729dbaa73c3a74806a3335b5c19", "577bb7476ed8beb7737fd341f8a62fd3");

            // Send the message using Twilio
            var messageResponse = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber("+16814333196"),
                to: new PhoneNumber("+91"+phoneNumber)
            );
            // Optionally, you can log the SID or message details if you want
            Console.WriteLine($"Message SID: {messageResponse.Sid}");
        }

        private async Task SendInterviewScheduleEmail(string subject,string toEmail, string googleMeetLink)
        {
            // Define the email subject and body
            string body = $"Dear Candidate,\n\nYour interview has been scheduled. Please use the following Google Meet link to join the interview:\n{googleMeetLink}\n\nBest regards,\nThe Hiring Team";

            // Send the email using the email service
            await _emailService.SendInterviewEmail(subject,toEmail, body);
        }

        public async Task<IActionResult> ScheduleInterviewEdit(int Id)
        {
            CandidateInterviewVm model = new CandidateInterviewVm();
            model = await _candidateInterview.GetCandidateInterviewScheduled(Id);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ScheduleInterviewEditSave(CandidateInterviewVm model)
        {
            string googleMeetLink = GenerateGoogleMeetLink();

            CandidateInterviewRoundScheduled candidateIRS = new CandidateInterviewRoundScheduled
            {
                Id = model.candidateInterviewRoundScheduled.Id,
                CandidateId = model.candidateInterviewRoundScheduled.CandidateId,
                IsCompleted = false,
                IsRescheduled = model.candidateInterviewRoundScheduled.IsRescheduled  ,
                RoundId = model.candidateInterviewRoundScheduled.RoundId,
                ScheduledDate = model.candidateInterviewRoundScheduled.ScheduledDate,
                GoogleMeetLink = string.IsNullOrEmpty(model.candidateInterviewRoundScheduled.GoogleMeetLink)
                    ? googleMeetLink
                    : model.candidateInterviewRoundScheduled.GoogleMeetLink,
            };

            await _candidateInterview.CandidateInterviewScheduledSave(candidateIRS);

            if (model.candidateInterviewRoundScheduled.IsRescheduled == true)
            {
                CandidateReScheduledInterview candidateRes = new CandidateReScheduledInterview
                {
                    Id = model.candidateReScheduledInterviewRoundScheduled.Id,
                    ReScheduledGoogleMeetLink = googleMeetLink,
                    CandidateInterviewScheduledId = model.candidateInterviewRoundScheduled.Id,
                    ReScheduledDate = model.candidateReScheduledInterviewRoundScheduled.ReScheduledDate,
                };
                await _candidateInterview.CandidateInterviewReScheduledSave(candidateRes);
            }
            var candidate = await _candidate.GetCandidate(model.CandidateId);
            var interviewRound = await _candidateInterview.GetInterviewRoundById(model.candidateInterviewRoundScheduled.RoundId);
            if (interviewRound == null)
            {
                // Handle case where the round name could not be found (optional)
                TempData["ErrorMessage"] = "Invalid interview round.";
                return RedirectToAction(nameof(ScheduleInterviewList));
            }
            //if (!string.IsNullOrEmpty(candidateEmail))
            //{
            //    string subject = "Your Interview is Scheduled";


            //    // Send the interview schedule email with the Google Meet link
            //    await SendInterviewScheduleEmail(subject, candidateEmail, googleMeetLink);
            //}
            //if (!string.IsNullOrEmpty(candidate.PhoneNumber))
            //{
            //    string roundName = interviewRound.RoundName;  // Assuming 'Name' is the field that holds the round name
            //    string rescheduleLink = Url.Action("ScheduleInterviewEdit", "CandidateInterview", new { Id = model.candidateInterviewRoundScheduled.Id }, Request.Scheme);
            //    string message = $"Hello {candidate.FirstName} {candidate.LastName}, your {roundName} interview has been Re-scheduled on {model.candidateReScheduledInterviewRoundScheduled.ReScheduledDate}. Please join the interview using the following Google Meet link: {googleMeetLink}. If you need to reschedule, click here: {rescheduleLink}.";

            //    // Send a text message with the Google Meet link
            //    await SendTextMessage(candidate.PhoneNumber, message);
            //}
            return RedirectToAction(nameof(ScheduleInterviewList));
        }

        private string GenerateGoogleMeetLink()
        {
            return "https://meet.google.com/" + Guid.NewGuid().ToString().Substring(0, 10);  
        }
 
        [HttpPost]
        public async Task<IActionResult> DeleteCandidateInterviewRound(int Id)
        {
            CandidateInterviewVm model = new CandidateInterviewVm();
            model = await _candidateInterview.GetCandidateInterviewScheduled(Id);

            if (model.candidateInterviewRoundScheduled.IsRescheduled == true)
            {
               await _candidateInterview.CandidateInterviewReScheduledDelete(Id);  
            }
            await _candidateInterview.CandidateInterviewScheduledDelete(Id);

            return RedirectToAction(nameof(ScheduleInterviewList));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAllCandidateInterviews(int CandidateId)
        {
            try
            {
                // Fetch all the scheduled interviews for the candidate
                List<CandidateInterviewVm> modelList = await _candidateInterview.GetCandidateInterviewsScheduledByCandidateId(CandidateId);

                // If no interviews are found for the candidate, return a Not Found status
                if (modelList == null || !modelList.Any())
                {
                    return NotFound("No interviews found for the given candidate.");
                }

                // Check if there are rescheduled interviews and delete them
                bool isReScheduledDeleted = await _candidateInterview.CandidateAllInterviewReScheduledDelete(CandidateId);

                bool isScheduledDeleted = await _candidateInterview.CandidateAllInterviewScheduledDelete(CandidateId);

                if (isScheduledDeleted)
                {
                    return RedirectToAction(nameof(ScheduleInterviewList));
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
        public JsonResult GetAllInterviews(int candidateId)
        {
            var candidate = _candidateInterview.GetCandidateWithInterviews(candidateId);

            return Json(candidate);
        }
        [HttpGet]
        public  JsonResult GetInterviewsDetailsById(int Id)
        {
            var model =  _candidateInterview.GetCandidateInterviewScheduledById(Id);

            return Json(model);
        }



        //All

        [HttpGet]
        public async Task<IActionResult> ScheduleInterviewEditAll(int CandidateId)
        {
            var candidateInterviewVm = _candidateInterview.GetCandidateWithInterviews(CandidateId);

            if (candidateInterviewVm == null)
            {
                return await Task.FromResult(NotFound());  // Ensure NotFound is wrapped as a Task
            }

            return View(candidateInterviewVm);  // Pass data to the view
        }
        [HttpPost]
        public IActionResult EditInterviewSchedule(CandidateInterviewVm model)
        {
            _candidateInterview.CandidateInterviewScheduledAllSave(model);
           
            return RedirectToAction(nameof(ScheduleInterviewList));
        }
    }
}
