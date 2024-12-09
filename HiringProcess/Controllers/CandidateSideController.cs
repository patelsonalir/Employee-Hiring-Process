using HiringProcess.Data;
using HiringProcess.Models;
using HiringProcess.Models.View_Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace HiringProcess.Controllers
{
    public class CandidateSideController : Controller
    {
        public ApplicationDbContext _dbContext { get; set; }
        public CandidateSideController(ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult AllQuizCompleted()
        {
            return View();
        }

        public IActionResult CandidateLogin()
        {
            return View();
        }       
        public IActionResult AlreadyCompletedQuizRound()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CandidateLogin(Candidate candidate)
        {
            var model = (from m in _dbContext.Candidate where m.CandidateName == candidate.CandidateName select m).Any();

            if (model)
            {
                var loginInfo = _dbContext.Candidate.Where(x => x.CandidateName == candidate.CandidateName).FirstOrDefault();
                HttpContext.Session.SetString("username", loginInfo.CandidateName);
                HttpContext.Session.SetString("Id", loginInfo.Id.ToString());

                return RedirectToAction("CandidateProfileAndQuizList", "CandidateSide");
            }
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> CandidateProfileAndQuizList()
        {
            var username = HttpContext.Session.GetString("username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("CandidateLogin");
            }

            string candidateId = HttpContext.Session.GetString("Id");

            var candidate = await _dbContext.Candidate.Include(c => c.Position).ThenInclude(p => p.Quiz).FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(candidateId));
            if (candidate == null)
            {
                return RedirectToAction("CandidateLogin");
            }

            var interviewRoundResult = await _dbContext.CandidateInterviewRoundResult.FirstOrDefaultAsync(cir => cir.CandidateId == candidate.Id && cir.InterviewRoundId == 1);

            // If the candidate has already completed the interview round, redirect to "AlreadyCompletedRound" page
            if (interviewRoundResult != null)
            {
                ViewBag.CandidateName = candidate.FirstName + " " + candidate.LastName;
                HttpContext.Session.Clear();
                return RedirectToAction("AlreadyCompletedQuizRound");
            }

            var model = new PositionQuizVM
            {
                PositionId = candidate.PositionId,
                CandidateName = $"{candidate.FirstName} {candidate.LastName}",
                Email = candidate.Email,
                ProfilePic = candidate.ProfilePic,
                Gender = candidate.Gender == "F" ? "Female" : "Male",
                ExperinceYearMonth = $"{candidate.ExperinceYear} ( {candidate.ExperinceMonth} )",
                PositionName = candidate.Position.Name,
                Quizzes = new List<Quiz>(),
                CandidateQuizResults = new List<CandidateQuizResult>(),
                QuizDisabledStatus = new List<bool>()
            };

            // Get quizzes for the position
            var positionQuizzes = await _dbContext.Position.Where(p => p.Id == candidate.PositionId).Select(p => p.Quiz).ToListAsync();

            // Get candidate's quiz results
            var candidateQuizResults = await _dbContext.CandidateQuizResult.Where(p => p.CandidateId == candidate.Id).ToListAsync();

            // Add quizzes and results to the model
            model.Quizzes.AddRange(positionQuizzes);
            model.CandidateQuizResults.AddRange(candidateQuizResults);

            // Loop through each quiz to check if it has at least one question
            foreach (var quiz in positionQuizzes)
            {
                // Query to check if the quiz has at least one question
                var quizQuestionsCount = await _dbContext.QuizQuestionAnswer.CountAsync(q => q.QuizID == quiz.Id);

                // Check if the quiz result already exists
                var existingResult = candidateQuizResults.FirstOrDefault(cr => cr.QuizId == quiz.Id);

                if (quizQuestionsCount > 0) // If the quiz has at least one question
                {
                    if (existingResult == null)
                    {
                        // Add a new result if it doesn't exist already
                        var newResult = new CandidateQuizResult
                        {
                            CandidateScore = 0,
                            CandidateOutOfMark = 0,
                            QuizId = quiz.Id,
                            CandidateId = Convert.ToInt32(candidateId),
                            HasTakenQuiz = false,
                            DateTaken = DateTime.Now,
                            Remarks = "",
                            CandidateQuizTimeTaken = TimeSpan.Parse("00:00")
                        };
                        _dbContext.CandidateQuizResult.Add(newResult);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        // Optionally, update existing result if needed (e.g., reset remarks or other fields)
                        existingResult.Remarks = ""; // Reset remarks if any
                        _dbContext.CandidateQuizResult.Update(existingResult);
                        await _dbContext.SaveChangesAsync();
                    }

                    model.QuizDisabledStatus.Add(existingResult?.HasTakenQuiz ?? false);
                    model.isNoQuestion.Add(0);
                }
                else
                {
                    // Handle the case where the quiz has no questions
                    model.QuizDisabledStatus.Add(true); // Disable the quiz
                    model.isNoQuestion.Add(1);

                    // If there's no existing result, create a new one with the remark "Quiz has no questions"
                    if (existingResult == null)
                    {
                        var newResult = new CandidateQuizResult
                        {
                            CandidateScore = 0,
                            CandidateOutOfMark = 0,
                            QuizId = quiz.Id,
                            CandidateId = Convert.ToInt32(candidateId),
                            HasTakenQuiz = false,
                            DateTaken = DateTime.Now,
                            Remarks = "Quiz has no questions", // Set the remark
                            CandidateQuizTimeTaken = TimeSpan.Parse("00:00")
                        };
                        _dbContext.CandidateQuizResult.Add(newResult);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        // Update the remark if the quiz already exists
                        existingResult.Remarks = "Quiz has no questions"; // Set the remark
                        _dbContext.CandidateQuizResult.Update(existingResult);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }

            // Update candidate quiz results
            var updatedCandidateQuizResults = await _dbContext.CandidateQuizResult.Where(cr => cr.CandidateId == Convert.ToInt32(candidateId)).ToListAsync();

            // Check if all quizzes are taken
            bool allQuizzesTaken = updatedCandidateQuizResults.All(cr => cr.HasTakenQuiz);

            if (allQuizzesTaken)
            {
                ViewBag.CandidateName = candidate.FirstName + " " + candidate.LastName;
                HttpContext.Session.Clear();
                return RedirectToAction("AllQuizCompleted");
            }

            return View(model);
        }



        [HttpGet]

        public IActionResult StartQuiz(int QuizId, string QuizName)
        {
            var username = HttpContext.Session.GetString("username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("CandidateLogin");
            }

            var model = new QuizViewModel
            {
                QuizId = QuizId,
                QuizName = QuizName
            };

            return View(model);
        }
        [HttpGet]

        public async Task<IActionResult> QuizQuestionAnswerList(int quizId)
        {
            var username = HttpContext.Session.GetString("username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("CandidateLogin");
            }
            var QuizQuestionAnswerList = _dbContext.QuizQuestionAnswer.Where(x => x.QuizID == quizId).ToList();

            var viewModel = QuizQuestionAnswerList.Select((item, index) => new QuizQuestionAnswerVM
             {
                numb = index + 1,
                question = item.QuestionText,
                answer = item.CorrectAnswer,
                options = new List<string>
                {
                    item.Option_A,
                    item.Option_B,
                    item.Option_C,
                    item.Option_D
                },
                QuestionWithImage = item.QuestionPicturePath
            }).ToList();
            
            return Json(new { data = viewModel });

        }
        [HttpGet]
        public async Task<IActionResult> CandidateQuizResultSave(int candidateScore, int candidateOutOfmark, int QuizId, string quizComplatedInTimeDuration)
        {
            try
            {
                var username = HttpContext.Session.GetString("username");

                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("CandidateLogin");
                }

                string candidateId = HttpContext.Session.GetString("Id");
                int CandidateIsAvailble = await _dbContext.CandidateQuizResult.Where(x => x.CandidateId == Convert.ToInt32(candidateId) && x.QuizId == QuizId).CountAsync();

                if (quizComplatedInTimeDuration == "00:00") { quizComplatedInTimeDuration = "10:00"; }

                if (CandidateIsAvailble > 0)
                {
                    CandidateQuizResult CandidateQuizResult = await _dbContext.CandidateQuizResult.FirstOrDefaultAsync(x => x.CandidateId == Convert.ToInt32(candidateId) && x.QuizId == QuizId);
                    if (CandidateQuizResult != null)
                    {
                        CandidateQuizResult CandidateQuizResultSave = new CandidateQuizResult();

                        CandidateQuizResultSave.CandidateScore = candidateScore;
                        CandidateQuizResultSave.CandidateOutOfMark = candidateOutOfmark;
                        CandidateQuizResultSave.HasTakenQuiz = true;
                        CandidateQuizResultSave.QuizId = CandidateQuizResult.QuizId;
                        CandidateQuizResultSave.Id = CandidateQuizResult.Id;
                        CandidateQuizResultSave.CandidateId = CandidateQuizResult.CandidateId;
                        CandidateQuizResultSave.DateTaken = DateTime.Now;
                        CandidateQuizResultSave.CandidateQuizTimeTaken = TimeSpan.Parse(quizComplatedInTimeDuration);

                        // Set Remarks
                        if (string.IsNullOrEmpty(CandidateQuizResult.Remarks))
                        {
                            // Check if the quiz has no questions and set Remarks accordingly
                            var quizQuestionsCount = await _dbContext.QuizQuestionAnswer.CountAsync(q => q.QuizID == QuizId);
                            if (quizQuestionsCount == 0)
                            {
                                CandidateQuizResultSave.Remarks = "Quiz has no questions"; // Set remarks if no questions
                            }
                            else
                            {
                                CandidateQuizResultSave.Remarks = CandidateQuizResult.Remarks; // Keep existing remarks if quiz has questions
                            }
                        }
                        else
                        {
                            CandidateQuizResultSave.Remarks = CandidateQuizResult.Remarks; // Preserve existing remarks
                        }

                        _dbContext.CandidateQuizResult.Update(CandidateQuizResultSave);
                        await _dbContext.SaveChangesAsync();
                    }

                    var CandidateQuizResults = await _dbContext.CandidateQuizResult.Where(cr => cr.CandidateId == Convert.ToInt32(candidateId) && cr.Remarks != "Quiz has no questions").ToListAsync();
                    var interviewRound = await _dbContext.InterviewRound.FirstOrDefaultAsync(ir => ir.RoundName.ToLower() == "quiz");

                    bool allQuizzesTaken = CandidateQuizResults.All(cr => cr.HasTakenQuiz);
                    if (allQuizzesTaken)
                    {
                        var totalScore = CandidateQuizResults.Sum(cr => cr.CandidateScore);
                        var totalOutOfMark = CandidateQuizResults.Sum(cr => cr.CandidateOutOfMark);

                        var candidateInterviewResult = new CandidateInterviewRoundResult
                        {
                            CandidateId = Convert.ToInt32(candidateId),
                            CandidateScore = totalScore,
                            CandidateOutOfScore = totalOutOfMark,
                            IsEligible = totalScore >= (totalOutOfMark * 0.6),
                            InterviewRoundId = interviewRound.Id,
                            InterviewDate = DateTime.Now,
                        };

                        await _dbContext.CandidateInterviewRoundResult.AddAsync(candidateInterviewResult);
                        await _dbContext.SaveChangesAsync();

                        var candidate = await _dbContext.Candidate.FindAsync(Convert.ToInt32(candidateId));

                        if (candidate != null)
                        {
                            candidate.Status = (candidateInterviewResult.IsEligible ?? false)
                                                ? interviewRound.RoundName + " Pass"
                                                : interviewRound.RoundName + " Fail";
                            _dbContext.Candidate.Update(candidate);
                            await _dbContext.SaveChangesAsync();
                        }
                    }
                    return Json(new { data = "Success" });
                }
                else
                {
                    return Json(new { data = "Fail" });
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
            return Json(new { data = "Fail" });
        }
    }
}
