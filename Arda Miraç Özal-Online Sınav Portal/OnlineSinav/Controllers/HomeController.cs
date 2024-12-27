using System.Diagnostics;
using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OnlineSinav.Models;
using Microsoft.AspNetCore.Identity;
using OnlineSinav.Repositories;
using System.Security.Claims;

namespace OnlineSinav.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IMapper _mapper;
        private readonly INotyfService _notfy;
        private readonly QuizRepository _quizRepository;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IMapper mapper, INotyfService notfy, QuizRepository quizRepository, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _mapper = mapper;
            _notfy = notfy;
            _quizRepository = quizRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Ogrenci"))
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var student = await _userManager.FindByIdAsync(userId);
                
                if(student == null)
                {
                    return RedirectToAction("Login", "User");
                }
                // Sadece aktif sınavları getir
                var activeQuizzes = await _quizRepository.GetActiveQuizzesByClassroomIdAsync(student.ClassroomId);
                return View(activeQuizzes);
            }
            else if (User.IsInRole("Teacher") || User.IsInRole("Admin"))
            {
                // Öğretmen veya admin için dashboard'a yönlendir
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                // Diğer roller için boş liste gönder
                return View(new List<Quiz>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
