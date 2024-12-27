using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OnlineSinav.Repositories;

namespace AnketPortali.Controllers
{
    [Authorize(Roles="Admin,Teacher")]
    public class AdminController : Controller
    {
       
        private readonly INotyfService _notyfService;
        private readonly IMapper _mapper;
        private readonly UserRepository _userRepository;

        public AdminController(INotyfService notyfService, IMapper mapper, UserRepository userRepository)
        {
            _notyfService = notyfService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            return View();
        }
       

       
    }
}
