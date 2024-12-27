using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using OnlineSinav.Hubs;
using OnlineSinav.Models;
using OnlineSinav.Repositories;
using OnlineSinav.ViewModel;

namespace OnlineSinav.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly QuizRepository _quizRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly ClassroomRepository _classroomRepository;
        private readonly QuestionRepository _questionRepository;
        private readonly StudentQuizRepository _studentQuizRepository;
        private readonly IHubContext<QuizHub> _hubContext;

        public QuizController(
            QuizRepository quizRepository,
            UserManager<AppUser> userManager,
            ClassroomRepository classroomRepository,
            QuestionRepository questionRepository,
            StudentQuizRepository studentQuizRepository,
            IHubContext<QuizHub> hubContext)
        {
            _quizRepository = quizRepository;
            _userManager = userManager;
            _classroomRepository = classroomRepository;
            _questionRepository = questionRepository;
            _studentQuizRepository = studentQuizRepository;
            _hubContext = hubContext;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (User.IsInRole("Teacher"))
            {
                // Öğretmen ise sadece kendi sınavlarını getir
                var teacherQuizzes = await _quizRepository.GetQuizzesByTeacherIdAsync(userId);
                return View(teacherQuizzes);
            }
            else if (User.IsInRole("Admin"))
            {
                // Admin ise tüm sınavları getir
                var allQuizzes = await _quizRepository.GetAllWithDetailsAsync();
                return View(allQuizzes);
            }
            else
            {
                // Öğrenci ise ana sayfaya yönlendir
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Classrooms = await _classroomRepository.GetAllAsync();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Create(Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                quiz.TeacherId = _userManager.GetUserId(User);
                await _quizRepository.AddAsync(quiz);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Classrooms = await _classroomRepository.GetAllAsync();
            return View(quiz);
        }

        public async Task<IActionResult> Details(int id)
        {
            var quiz = await _quizRepository.GetByIdWithDetailsAsync(id);
            if (quiz == null)
                return NotFound();

            var questions = await _questionRepository.GetQuestionsByQuizIdAsync(id);

            var viewModel = new QuizDetailsViewModel
            {
                Quiz = quiz,
                Questions = questions
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var quiz = await _quizRepository.GetByIdWithDetailsAsync(id);
            if (quiz == null)
                return NotFound();

            // Öğretmen sadece kendi sınavını düzenleyebilir
            if (User.IsInRole("Teacher") && quiz.TeacherId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Unauthorized();

            var classrooms = await _classroomRepository.GetAllAsync();
            
            var viewModel = new EditQuizViewModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = "Sınav",
                StartDate = quiz.StartDate,
                EndDate = quiz.EndDate,
                ClassroomId = quiz.ClassroomId,
                AvailableClassrooms = classrooms.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Edit(EditQuizViewModel model)
        {
            

            var quiz = await _quizRepository.GetByIdAsync(model.Id);
            if (quiz == null)
                return NotFound();

            // Öğretmen sadece kendi sınavını düzenleyebilir
            if (User.IsInRole("Teacher") && quiz.TeacherId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Unauthorized();

            quiz.Title = model.Title;
           
            quiz.StartDate = model.StartDate;
            quiz.EndDate = model.EndDate;
            quiz.ClassroomId = model.ClassroomId;

            await _quizRepository.UpdateAsync(quiz);
            
            TempData["SuccessMessage"] = "Sınav başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetClassrooms()
        {
            var classrooms = await _classroomRepository.GetAllAsync();
            return Json(classrooms.Select(c => new { id = c.Id, name = c.Name }));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var quiz = new Quiz
                    {
                        Title = model.Title,
                        ClassroomId = model.ClassroomId,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        IsActive = model.IsActive,
                        TeacherId = _userManager.GetUserId(User)
                    };

                    await _quizRepository.AddAsync(quiz);
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _quizRepository.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Questions(int id)
        {
            var quiz = await _quizRepository.GetByIdWithDetailsAsync(id);
            if (quiz == null)
                return NotFound();

            return View(quiz);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateModel model)
        {
            try
            {
                var question = new Question
                {
                    QuizId = model.QuizId,
                    Text = model.Text,
                    Options = model.Options.Select(o => new Option
                    {
                        Text = o.Text,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                };

                var quiz = await _quizRepository.GetByIdAsync(model.QuizId);
                if (quiz == null)
                    return Json(new { success = false });

                quiz.Questions ??= new List<Question>();
                quiz.Questions.Add(question);
                await _quizRepository.UpdateAsync(quiz);

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var result = await _questionRepository.DeleteQuestionWithOptionsAsync(id);
                return Json(new { success = result });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [Authorize]
        public async Task<IActionResult> TakeQuiz(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var quiz = await _quizRepository.GetByIdWithDetailsAsync(id);

                if (quiz == null || quiz.ClassroomId != user.ClassroomId)
                    return NotFound();

                if (DateTime.Now < quiz.StartDate || DateTime.Now > quiz.EndDate)
                    return RedirectToAction("Index", "Home");

                var hasCompleted = await _studentQuizRepository.HasStudentCompletedQuiz(user.Id, id);
                if (hasCompleted)
                    return RedirectToAction("QuizResult", new { quizId = id });

                var studentQuiz = await _studentQuizRepository.GetByStudentAndQuizId(user.Id, id)
                    ?? await _studentQuizRepository.StartQuiz(user.Id, id);

                var questions = await _questionRepository.GetQuestionsByQuizIdAsync(id);
                if (!questions.Any())
                    return RedirectToAction("Index", "Home");

                var viewModel = new TakeQuizViewModel
                {
                    StudentQuizId = studentQuiz.Id,
                    QuizTitle = quiz.Title,
                    Questions = questions,
                    TotalQuestions = questions.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionModel model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var studentQuiz = await _studentQuizRepository.GetByIdAsync(model.StudentQuizId);

                if (studentQuiz == null || studentQuiz.StudentId != userId)
                    return Json(new { success = false });

                foreach (var answer in model.Answers)
                {
                    await _studentQuizRepository.SaveAnswer(
                        model.StudentQuizId,
                        answer.QuestionId,
                        answer.SelectedOptionId);
                }

                await _studentQuizRepository.CompleteQuiz(model.StudentQuizId);

                // Öğrenci bilgilerini al
                var student = await _userManager.FindByIdAsync(userId);
                var quiz = await _quizRepository.GetByIdAsync(studentQuiz.QuizId);

                // SignalR ile bildirim gönder
                await SendQuizCompletionNotification(student, quiz);

                return Json(new { success = true, quizId = studentQuiz.QuizId });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        private async Task SendQuizCompletionNotification(AppUser student, Quiz quiz)
        {
            var notification = new
            {
                studentName = student.FirstName + student.LastName,
                quizTitle = quiz.Title,
                quizId = quiz.Id,
                studentId = student.Id,
                completedAt = DateTime.Now,
                score = await _studentQuizRepository.CalculateScore(quiz.Id, student.Id)
            };

            // Öğretmene bildirim gönder
            await _hubContext.Clients.Group($"Teacher_{quiz.TeacherId}")
                .SendAsync("QuizCompleted", notification);

            // Adminlere bildirim gönder
            await _hubContext.Clients.Group("Admins")
                .SendAsync("QuizCompleted", notification);
        }

        [HttpGet]
       
        [Route("Quiz/GetQuizQuestions/{studentQuizId}")]
        public async Task<IActionResult> GetQuizQuestions(int studentQuizId)
        {
            try 
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var studentQuiz = await _studentQuizRepository.GetByIdAsync(studentQuizId);
                if (studentQuiz == null || studentQuiz.StudentId != userId)
                    return NotFound();

                var questions = await _questionRepository.GetQuestionsByQuizIdAsync(studentQuiz.QuizId);
                
                if (!questions.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = $"Quiz ID {studentQuiz.QuizId} için soru bulunamadı",
                        debug = new { 
                            studentQuizId = studentQuizId,
                            quizId = studentQuiz.QuizId,
                            userId = userId
                        }
                    });
                }

                return Json(new { 
                    success = true, 
                    questions = questions,
                    debug = new { 
                        questionCount = questions.Count,
                        firstQuestionId = questions.FirstOrDefault()?.Id,
                        quizId = studentQuiz.QuizId
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Sorular yüklenirken bir hata oluştu",
                    error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] AnswerSubmissionModel model)
        {
            //try
            //{
            //    await _studentQuizRepository.SaveAnswer(
            //        model.StudentQuizId, 
            //        model.QuestionId, 
            //        model.SelectedOptionId);
            //    return Json(new { success = true });
            //}
            //catch
            //{
            //    return Json(new { success = false });
            //}
            try
            {
                var studentQuiz = await _studentQuizRepository.GetByIdAsync(model.StudentQuizId);
                if (studentQuiz == null)
                    return Json(new { success = false });

                var answer = new StudentAnswer
                {
                    StudentQuizId = model.StudentQuizId,
                    QuestionId = model.QuestionId,
                    SelectedOptionId = model.SelectedOptionId,
                    AnsweredAt = DateTime.UtcNow,
                    
                };

                // Answers koleksiyonuna ekleme
                studentQuiz.Answers.Add(answer);
                await _studentQuizRepository.UpdateAsync(studentQuiz);

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteQuiz(int studentQuizId)
        {
            try
            {
                await _studentQuizRepository.CompleteQuiz(studentQuizId);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [Authorize]
        public async Task<IActionResult> QuizResult(int quizId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var studentQuiz = await _studentQuizRepository.GetQuizResultAsync(userId, quizId);

                if (studentQuiz == null)
                    return NotFound();

                var viewModel = new QuizResultViewModel
                {
                    QuizTitle = studentQuiz.Quiz.Title,
                    Score = studentQuiz.Score,
                    TotalQuestions = studentQuiz.Quiz.Questions.Count,
                    CompletedAt = studentQuiz.CompletedAt.Value,
                    Answers = studentQuiz.Answers.Select(a => new QuizResultAnswerViewModel
                    {
                        QuestionText = a.Question.Text,
                        SelectedOptionText = a.SelectedOption.Text,
                        IsCorrect = a.SelectedOption.IsCorrect,
                        CorrectOptionText = a.Question.Options.FirstOrDefault(o => o.IsCorrect)?.Text
                    }).ToList()
                };

                return View(viewModel);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> Results(int quizId)
        {
            var quiz = await _quizRepository.GetByIdWithDetailsAsync(quizId);
            if (quiz == null)
                return NotFound();

            if (User.IsInRole("Teacher") && quiz.TeacherId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                return Unauthorized();

            var results = await _studentQuizRepository.GetQuizResultsAsync(quizId);
            
            var viewModel = new QuizResultsViewModel
            {
                QuizId = quizId,
                QuizTitle = quiz.Title,
                ClassroomName = quiz.Classroom?.Name,
                StartDate = quiz.StartDate,
                EndDate = quiz.EndDate,
                TotalQuestions = quiz.Questions.Count,
                Results = results.Select(sq => new StudentResultViewModel
                {
                    StudentId = sq.StudentId,
                    StudentName = sq.Student.FirstName + sq.Student.LastName,
                    Score = sq.Score,
                    CompletedAt = sq.CompletedAt.Value,
                    CorrectAnswers = sq.Answers.Count(a => a.SelectedOption.IsCorrect),
                    WrongAnswers = sq.Answers.Count(a => !a.SelectedOption.IsCorrect)
                }).OrderByDescending(r => r.Score).ToList()
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> StudentDetail(int quizId, string studentId)
        {
            try
            {
                var quiz = await _quizRepository.GetByIdWithDetailsAsync(quizId);
                if (quiz == null)
                    return NotFound();

                // Öğretmenler sadece kendi sınavlarını görebilsin
                if (User.IsInRole("Teacher") && quiz.TeacherId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                    return Unauthorized();

                var studentQuiz = await _studentQuizRepository.GetStudentQuizDetailAsync(quizId, studentId);
                if (studentQuiz == null)
                    return NotFound();

                var viewModel = new StudentQuizDetailViewModel
                {
                    QuizTitle = quiz.Title,
                    StudentName = studentQuiz.Student.FirstName + studentQuiz.Student.LastName,
                    Score = studentQuiz.Score,
                    CompletedAt = studentQuiz.CompletedAt.Value,
                    TotalQuestions = quiz.Questions.Count,
                    CorrectAnswers = studentQuiz.Answers.Count(a => a.SelectedOption.IsCorrect),
                    WrongAnswers = studentQuiz.Answers.Count(a => !a.SelectedOption.IsCorrect),
                    Answers = studentQuiz.Answers.Select(a => new StudentAnswerDetailViewModel
                    {
                        QuestionText = a.Question.Text,
                        SelectedOptionText = a.SelectedOption.Text,
                        IsCorrect = a.SelectedOption.IsCorrect,
                        CorrectOptionText = a.Question.Options.First(o => o.IsCorrect).Text,
                        AllOptions = a.Question.Options.Select(o => new OptionDetailViewModel
                        {
                            Text = o.Text,
                            IsSelected = o.Id == a.SelectedOptionId,
                            IsCorrect = o.IsCorrect
                        }).ToList()
                    }).ToList()
                };

                return View(viewModel);
            }
            catch
            {
                return RedirectToAction("Results", new { quizId });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var quiz = await _quizRepository.GetByIdAsync(id);
                if (quiz == null)
                    return Json(new { success = false, message = "Sınav bulunamadı." });

                // Öğretmen sadece kendi sınavını düzenleyebilir
                if (User.IsInRole("Teacher") && quiz.TeacherId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                    return Json(new { success = false, message = "Yetkisiz işlem." });

                quiz.IsActive = !quiz.IsActive;
                await _quizRepository.UpdateAsync(quiz);

                return Json(new { 
                    success = true, 
                    message = quiz.IsActive ? "Sınav aktif edildi." : "Sınav pasif edildi.",
                    isActive = quiz.IsActive 
                });
            }
            catch
            {
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }

      

       

       

       

       

       
      


      

        
    }
}


   
  