using HiringProcess.Data;
using HiringProcess.Interface;
using HiringProcess.Models;
using HiringProcess.Models.View_Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HiringProcess.Controllers
{
    public class QuestionAnswerController : MasterController
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IQuestionAnswer _questionAnswer;
        public QuestionAnswerController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment,IQuestionAnswer questionAnswer)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _questionAnswer = questionAnswer;   
        }

        public async Task<IActionResult> QuizQuestionAnswerList(string sKey = "", int currentPage = 1, string sortColumn = "", string sortOrder = "")
        {
            // Fetching active quizzes from the database
            var quizzes = _context.Quiz.Where(x => x.IsActive == true).ToList();
            ViewData["QuizData"] = quizzes;

            // Fetching quiz question answers with their associated quiz names
            var query = (from c in _context.QuizQuestionAnswer
                         join q in _context.Quiz on c.QuizID equals q.Id
                         select new QuizQuestionAnswer
                         {
                             QuestionText = c.QuestionText,
                             CorrectAnswer = c.CorrectAnswer,
                             Id = c.Id,
                             Option_A = c.Option_A,
                             Option_B = c.Option_B,
                             Option_C = c.Option_C,
                             Option_D = c.Option_D,
                             QuestionPicturePath = c.QuestionPicturePath,
                             QuizID = c.QuizID,
                             QuestionPictureContentType = c.QuestionPictureContentType,
                             QuizName = q.Name,  // Including the Quiz Name for the Question
                         });

            var results = await query.ToListAsync();

            // Grouping the result by QuizID
            var groupedResults = results
                .GroupBy(x => x.QuizID)
                .Select(group => new QuizQuestionAnswerViewModel
                {
                    QuizId = group.Key,
                    QuizName = group.First().QuizName,  // Get the Quiz Name from the first entry of the group
                    QuestionAnswers = group.ToList()   // List of quiz question answers
                }).ToList();

            // Passing the grouped results to the view
            return View(groupedResults);
        }

        public async Task<IActionResult> QuizQuestionAnswerEntry(int QuizQuestionAnswerId)
        {
            var Quiz = _context.Quiz.Where(x => x.IsActive == true).ToList();
            ViewData["QuizData"] = Quiz;

            QuizQuestionAnswer model = new QuizQuestionAnswer();
            if (QuizQuestionAnswerId == 0)
            {
            }
            else
            {
                var QuizQuestionAnswer = await _context.QuizQuestionAnswer.Include(x => x.QuizQuestionAnswers).FirstOrDefaultAsync(c => c.Id == QuizQuestionAnswerId);

                if (QuizQuestionAnswer == null)
                {
                    return NotFound();
                }

                // Map Category to CategoryViewModel
                var model1 = new QuizQuestionsAnswerEntryVM
                {
                    Id = QuizQuestionAnswer.Id,
                    QuizID = QuizQuestionAnswer.QuizID,
                    //QuizQuestionAnswers = QuizQuestionAnswer.QuizQuestionAnswers.Select(sc => new SubCategoryViewModel
                    //{
                    //    Name = sc.Name,
                    //    // Add any other properties you want to pass
                    //}).ToList()
                };

                return View(model);
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuizQuestionAnswerEntry(QuizQuestionsAnswerEntryVM model)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QuestionPicture/Quiz");

            foreach (var Question in model.QuizQuestionAnswers)
            {
                string correctAnswerText = string.Empty;

                // Determine the correct answer text based on the CorrectAnswer value
                switch (Question.CorrectAnswer.ToUpper())
                {
                    case "A":
                        correctAnswerText = Question.Option_A;
                        break;
                    case "B":
                        correctAnswerText = Question.Option_B;
                        break;
                    case "C":
                        correctAnswerText = Question.Option_C;
                        break;
                    case "D":
                        correctAnswerText = Question.Option_D;
                        break;
                    default:
                        correctAnswerText = "Invalid Option"; // Handle unexpected values
                        break;
                }

                var questions = new QuizQuestionAnswer
                {
                    QuestionText = Question.QuestionText,
                    Option_A = Question.Option_A,
                    Option_B = Question.Option_B,
                    Option_C = Question.Option_C,
                    Option_D = Question.Option_D,
                    CorrectAnswer = correctAnswerText,  // Save the correct answer text
                    QuizID = model.QuizID,
                    QuestionPictureContentType = Question.QuestionPicturePathIform == null ? "" : Question.QuestionPicturePathIform.ContentType,
                    QuestionPicturePath = Question.QuestionPicturePathIform == null ? "" : UploadedFile(Question.QuestionPicturePathIform, uploadsFolder)
                };

                _context.QuizQuestionAnswer.Add(questions);
                _context.SaveChanges();
            }

            var Quiz = _context.Quiz.Where(x => x.IsActive == true).ToList();
            ViewData["QuizData"] = Quiz;
            return RedirectToAction(nameof(QuizQuestionAnswerList));// Redirect to the Index page or another action
        }
        private string UploadedFile(IFormFile model, string uploadsFolder)
        {
            string uniqueFileName = null;


            if (model != null)
            {
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    model.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
        [HttpGet]

        public IActionResult GetAllQuestionsByQuiz(int quizId)
        {
            var questions = _context.QuizQuestionAnswer
                                    .Where(q => q.QuizID == quizId)
                                    .Select(q => new
                                    {
                                        q.QuestionText,
                                        q.CorrectAnswer,
                                        q.Option_A,
                                        q.Option_B,
                                        q.Option_C,
                                        q.Option_D,
                                        q.QuestionPicturePath
                                    })
                                    .ToList();

            return Json(questions);
        }
        [HttpGet]
        public IActionResult AllQuestionAnswerEdit(int QuiId)
        {
            var quiz = _context.Quiz
                               .Where(q => q.Id == QuiId)
                               .Include(q => q.QuizQuestions)  // Eagerly load the Questions
                               .FirstOrDefault(); // Fetch the Quiz and its Questions

            if (quiz == null)
            {
                return NotFound(); // If quiz not found, return 404
            }

            // Check if Questions is null, and initialize it if necessary
            if (quiz.QuizQuestions == null)
            {
                quiz.QuizQuestions = new List<QuizQuestionAnswer>(); // Initialize empty list if null
            }

            var viewModel = new QuizQuestionAnswerViewModel
            {
                QuizId = quiz.Id,
                QuizName = quiz.Name,
                // Safely project Questions into the view model
                QuestionAnswers = quiz.QuizQuestions
                                      .Select(qa => new QuizQuestionAnswer
                                      {
                                          Id = qa.Id,
                                          QuestionText = qa.QuestionText,
                                          Option_A = qa.Option_A,
                                          Option_B = qa.Option_B,
                                          Option_C = qa.Option_C,
                                          Option_D = qa.Option_D,
                                          CorrectAnswer = GetCorrectAnswer(qa), // Get the letter corresponding to the correct answer
                                          QuestionPicturePath = qa.QuestionPicturePath,
                                          QuizID = qa.QuizID,
                                          removeImage = qa.QuestionPicturePath == "" ? true : false,
                                      }).ToList() // Convert to list
            };

            return View(viewModel); // Pass the view model to the view
        }

        // Method to get the correct answer letter
        private string GetCorrectAnswer(QuizQuestionAnswer qa)
        {
            if (qa.CorrectAnswer == qa.Option_A)
                return "A";
            if (qa.CorrectAnswer == qa.Option_B)
                return "B";
            if (qa.CorrectAnswer == qa.Option_C)
                return "C";
            if (qa.CorrectAnswer == qa.Option_D)
                return "D";

            return string.Empty; // If none of the options match, return an empty string
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AllQuestionAnswerEditSave(QuizQuestionAnswerViewModel model)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QuestionPicture/Quiz");

            foreach (var Question in model.QuestionAnswers)
            {
                string correctAnswerText = string.Empty;

                // Determine the correct answer text based on the CorrectAnswer value
                switch (Question.CorrectAnswer.ToUpper())
                {
                    case "A":
                        correctAnswerText = Question.Option_A;
                        break;
                    case "B":
                        correctAnswerText = Question.Option_B;
                        break;
                    case "C":
                        correctAnswerText = Question.Option_C;
                        break;
                    case "D":
                        correctAnswerText = Question.Option_D;
                        break;
                    default:
                        correctAnswerText = "Invalid Option"; // Handle unexpected values
                        break;
                }

                string questionPicturePath = string.Empty;
                string questionPictureContentType = string.Empty;

                // Check if a new image is uploaded
                if (Question.QuestionPicturePathIform != null)
                {
                    questionPicturePath = UploadedFile(Question.QuestionPicturePathIform, uploadsFolder);
                    questionPictureContentType = Question.QuestionPicturePathIform.ContentType;
                }
                else
                {
                    // If no new image is uploaded, check if there was a previously saved image
                    QuizQuestionAnswer quizQuestionAnswer = _context.QuizQuestionAnswer.FirstOrDefault(x => x.Id == Question.Id);
                    if (quizQuestionAnswer != null)
                    {
                        if (Question.removeImage == true)
                        {
                            questionPicturePath = "";
                            questionPictureContentType = "";
                        }
                        else
                        {
                            questionPicturePath = quizQuestionAnswer.QuestionPicturePath;
                            questionPictureContentType = quizQuestionAnswer.QuestionPictureContentType;
                        }

                    }
                    else
                    {
                        // If no image exists, leave the values empty
                        questionPicturePath = "";
                        questionPictureContentType = "";
                    }
                }

                var questions = new QuizQuestionAnswer
                {
                    Id = Question.Id,
                    QuestionText = Question.QuestionText,
                    Option_A = Question.Option_A,
                    Option_B = Question.Option_B,
                    Option_C = Question.Option_C,
                    Option_D = Question.Option_D,
                    CorrectAnswer = correctAnswerText,  // Save the correct answer text
                    QuizID = Question.QuizID,
                    QuestionPictureContentType = questionPictureContentType,
                    QuestionPicturePath = questionPicturePath
                };

                _context.QuizQuestionAnswer.Update(questions);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(QuizQuestionAnswerList)); // Redirect to the Index page or another action
        }


        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int Id)
        {

            var models = _context.QuizQuestionAnswer.Where(c => c.Id == Id).ToList();

            if (models.Any())
            {
                _context.QuizQuestionAnswer.RemoveRange(models);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(QuizQuestionAnswerList));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAllQuestion(int QuizId)
        {
            //try
            //{
            var models = _context.QuizQuestionAnswer.Where(c => c.QuizID == QuizId).ToList();

            if (models.Any())
            {
                _context.QuizQuestionAnswer.RemoveRange(models);
                await _context.SaveChangesAsync();
            }


            //if (isDeleted)
            //{
            return RedirectToAction(nameof(QuizQuestionAnswerList));
            //    }
            //    else
            //    {
            //        return StatusCode(500, "An error occurred while deleting the interviews.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return StatusCode(500, "An error occurred while processing your request.");
            //}
        }
        [HttpPost]
        public async Task<IActionResult> EditQuestion(int questionId, string questionText, string optionA, string optionB, string optionC, string optionD, string correctAnswer, IFormFile questionImage, string previousImagePath, bool removeImage)
        {
            // Find the existing question by ID
            var question = await _context.QuizQuestionAnswer.FindAsync(questionId);
            if (question == null)
            {
                return NotFound();
            }

            string correctAnswerText = string.Empty;

            // Determine the correct answer text based on the CorrectAnswer value
            switch (correctAnswer.ToUpper())
            {
                case "A":
                    correctAnswerText = optionA;
                    break;
                case "B":
                    correctAnswerText = optionB;
                    break;
                case "C":
                    correctAnswerText = optionC;
                    break;
                case "D":
                    correctAnswerText = optionD;
                    break;
                default:
                    correctAnswerText = "Invalid Option"; // Handle unexpected values
                    break;
            }

            // Update the question fields
            question.QuestionText = questionText;
            question.Option_A = optionA;
            question.Option_B = optionB;
            question.Option_C = optionC;
            question.Option_D = optionD;
            question.CorrectAnswer = correctAnswerText;

            // Handle image upload or removal
            if (removeImage)
            {
                // Remove the image from the file system
                if (!string.IsNullOrEmpty(previousImagePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, previousImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath); // Delete the image file
                    }
                }

                // Set the QuestionPicturePath to null
                question.QuestionPicturePath = "";
                question.QuestionPictureContentType = ""; // Optionally clear content type as well
            }
            else if (questionImage != null)
            {
                // Save the new image to disk
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QuestionPicture/Quiz");
                question.QuestionPicturePath = UploadedFile(questionImage, uploadsFolder);
                question.QuestionPictureContentType = questionImage.ContentType;
            }
            else if (!string.IsNullOrEmpty(previousImagePath))
            {
                // If no new image is uploaded, retain the previous one
                question.QuestionPicturePath = previousImagePath;
            }

            // Save the changes to the database
            _context.Update(question);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(QuizQuestionAnswerList)); // Redirect to the Quiz Question List or another action
        }



        public async Task<IActionResult> PracticalExamQuestioList()
        {
            var Candidate = await _questionAnswer.PracticalExamQuestionGetList();


            return View(Candidate);
        }
        public async Task<IActionResult> PracticalExamQuestionEntry(int Id)
        {
            var Position = _context.Position.Where(x => x.IsActive == true).ToList();
            ViewData["PositionData"] = Position;
            PracticalExamQuestion practicalExam = new PracticalExamQuestion();
            if (Id != 0)
            {
                practicalExam = await _questionAnswer.GetPositionQuestionById(Id);
            }
            return View(practicalExam);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PracticalExamQuestionEntry(PracticalExamQuestion model)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QuestionPicture/PracticalExam");

            var questions = new PracticalExamQuestion
            {
                QuestionText = model.QuestionText,
                Id = model.Id,
                PositionId = model.PositionId,
                QuestionPictureContentType = model.QuestionPicturePathIform == null ? "" : model.QuestionPicturePathIform.ContentType,
                QuestionPicturePath = model.QuestionPicturePathIform == null ? "" : UploadedFile(model.QuestionPicturePathIform, uploadsFolder)
            };
            if (model.Id == 0)
            {
                _context.PracticalExamQuestion.Add(questions);
            }
            else
            {
                _context.PracticalExamQuestion.Update(questions);
            }
            _context.SaveChanges();

            var Position = _context.Position.Where(x => x.IsActive == true).ToList();
            ViewData["PositionData"] = Position;

            return RedirectToAction(nameof(PracticalExamQuestioList));// Redirect to the Index page or another action
        }

        [HttpPost]
        public async Task<IActionResult> DeletePracticalExamQuestionDetails(int Id)
        {

            _questionAnswer.DeletePracticalExamQuestion(Id);

            return RedirectToAction(nameof(PracticalExamQuestioList));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuizQuestionAnswerDetails(int QuizQuestionAnswerId)
        {

            _questionAnswer.DeleteQuizQuestionAnswer(QuizQuestionAnswerId);

            return RedirectToAction(nameof(QuizQuestionAnswerList));
        }
        public async Task<IActionResult> UploadExcelForQuizQuestionAnswer()
        {
            var viewModel = new QuestionsAnswerQuizVM
            {
                quizzes = await _context.Quiz.Where(x => x.IsActive == true).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadExcelForQuizQuestionAnswer(QuestionsAnswerQuizVM viewModel)
        {
            if (viewModel.Id == null || viewModel.Id == 0)
            {
                ViewData["Message"] = "No Quiz selected.";
                viewModel.quizzes = await _context.Quiz.Where(x => x.IsActive == true).ToListAsync();
                return View(viewModel);
            }

            if (viewModel.File == null || viewModel.File.Length == 0)
            {
                ViewData["Message"] = "No file selected.";
                viewModel.quizzes = await _context.Quiz.Where(x => x.IsActive == true).ToListAsync();
                return View(viewModel);
            }

            if (!viewModel.File.FileName.EndsWith(".xlsx"))
            {
                ViewData["Message"] = "Please upload a valid Excel file.";
                viewModel.quizzes = await _context.Quiz.Where(x => x.IsActive == true).ToListAsync();
                return View(viewModel);
            }

            try
            {
                var qa = new List<QuizQuestionAnswer>();

                using (var package = new ExcelPackage(viewModel.File.OpenReadStream()))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Use the first worksheet
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Assuming the first row is header
                    {

                        var correctAnswer = string.IsNullOrEmpty(worksheet.Cells[row, 6].Text) ? "" : worksheet.Cells[row, 6].Text; // Read and normalize the correct answer
                        string answerText = string.Empty;

                        // Validate the correct answer (only A, B, C, or D)
                        if (correctAnswer == "A" || correctAnswer == "a")
                        {
                            answerText = worksheet.Cells[row, 2].Text; // Option A
                        }
                        else if (correctAnswer == "B" || correctAnswer == "b")
                        {
                            answerText = worksheet.Cells[row, 3].Text; // Option B
                        }
                        else if (correctAnswer == "C" || correctAnswer == "c")
                        {
                            answerText = worksheet.Cells[row, 4].Text; // Option C
                        }
                        else if (correctAnswer == "D" || correctAnswer == "d")
                        {
                            answerText = worksheet.Cells[row, 5].Text; // Option D
                        }
                        else
                        {
                            // If correctAnswer is not A, B, C, or D, you can handle this case, e.g., set answerText to empty or null
                            answerText = correctAnswer;
                        }

                        var quan = new QuizQuestionAnswer
                        {
                            QuestionText = string.IsNullOrEmpty(worksheet.Cells[row, 1].Text) ? "" : worksheet.Cells[row, 1].Text,
                            Option_A = string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) ? "" : worksheet.Cells[row, 2].Text,
                            Option_B = string.IsNullOrEmpty(worksheet.Cells[row, 3].Text) ? "" : worksheet.Cells[row, 3].Text,
                            Option_C = string.IsNullOrEmpty(worksheet.Cells[row, 4].Text) ? "" : worksheet.Cells[row, 4].Text,
                            Option_D = string.IsNullOrEmpty(worksheet.Cells[row, 5].Text) ? "" : worksheet.Cells[row, 5].Text,
                            CorrectAnswer = answerText,
                            QuestionPicturePath = string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? "" : UploadedFileExcelImage(worksheet.Cells[row, 7].Text),
                            QuestionPictureContentType = string.IsNullOrEmpty(worksheet.Cells[row, 7].Text) ? "" : GetContentType(worksheet.Cells[row, 7].Text),
                            QuizID = viewModel.Id
                        };

                        qa.Add(quan);
                    }
                }

                await _context.QuizQuestionAnswer.AddRangeAsync(qa);
                await _context.SaveChangesAsync();

                ViewData["Message"] = $"{qa.Count} Question Answer uploaded successfully!";
                viewModel.quizzes = await _context.Quiz.Where(x => x.IsActive == true).ToListAsync();
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewData["Message"] = "An error occurred while processing the file: " + ex.Message;
                viewModel.quizzes = await _context.Quiz.Where(x => x.IsActive == true).ToListAsync();
                return View(viewModel);
            }
        }

        private string UploadedFileExcelImage(string Filepath)
        {
            string uniqueFileName = null;

            if (!string.IsNullOrEmpty(Filepath))
            {
                IFormFile formFile = ConvertToIFormFile(Filepath);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "QuestionPicture/Quiz");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + formFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    formFile.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
        private string GetContentType(string Filepath)
        {
            string uniqueFileNameContentType = null;

            if (!string.IsNullOrEmpty(Filepath))
            {
                IFormFile formFile = ConvertToIFormFile(Filepath);
                uniqueFileNameContentType = formFile.ContentType;
            }

            return uniqueFileNameContentType;
        }
        public static IFormFile ConvertToIFormFile(string filePath)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            var stream = new MemoryStream(fileBytes);

            string contentType;
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out contentType))
            {
                contentType = "application/octet-stream";
            }
            var formFile = new FormFile(stream, 0, fileBytes.Length, "file", Path.GetFileName(filePath))
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return formFile;
        }
        [HttpGet]
        public IActionResult DownloadSampleExcelFromQuestionAddExcel()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Question/Sample File", "Sample Excel From Question Add.xlsx");

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sample Excel From Question Add.xlsx");
        }
    }
}
