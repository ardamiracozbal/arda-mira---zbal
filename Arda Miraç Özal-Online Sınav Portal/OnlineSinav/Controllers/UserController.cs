using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineSinav.Models;
using OnlineSinav.Repositories;

namespace OnlineSinav.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly INotyfService _notyfService;
        private readonly UserRepository _userRepository;
        private readonly ClassroomRepository _classroomRepository;


        public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, INotyfService notyfService, UserRepository userRepository, ClassroomRepository classroomRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _notyfService = notyfService;
            _userRepository = userRepository;
            _classroomRepository = classroomRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {

                    if(user.IsApproved == false)
                    {
                        _notyfService.Information("Öğrenci Kayıt İşleminiz Tamamlanmadı Öğretmenizle iletişime geçiniz.");
                        return RedirectToAction("Login", "User");
                    }

                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                    
                    if (result.Succeeded)
                    {

                        var getuser = await _userManager.GetRolesAsync(user);

                        if (getuser.Contains("Admin"))
                        {
                            _notyfService.Success("Başarıyla giriş yaptınız!");
                            return RedirectToAction("Index", "Admin");
                        }
                        if (getuser.Contains("Teacher"))
                        {
                            _notyfService.Success("Başarıyla giriş yaptınız!");
                            return RedirectToAction("Index", "Admin");
                        }


                        _notyfService.Success("Başarıyla giriş yaptınız!");
                        return RedirectToAction("Index", "Home");
                    }
                }
                _notyfService.Error("Email veya şifre hatalı!");
                ModelState.AddModelError("", "Email veya şifre hatalı!");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    TCNo = model.TcNo,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsApproved=false,
                    ClassroomId=1,
                };
                try
                {
                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // Rol kontrolü ve atama
                        if (!await _roleManager.RoleExistsAsync("Ogrenci"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole { Name = "Ogrenci" });
                        }

                        await _userManager.AddToRoleAsync(user, "Ogrenci");

                        _notyfService.Information("Başarıyla kayıt oldunuz!");
                        return RedirectToAction("Login");
                    }


                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                        _notyfService.Error(error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _notyfService.Warning("Sistemsel hata oluştu tc doğru giriniz");
                    return RedirectToAction("Register", "User");
                }


               

               

              
            }
            return View(model);
        }
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _notyfService.Success("Başarıyla çıkış yaptınız!");
            return RedirectToAction("Login");
        }


        public async Task<IActionResult> User()
        {
            return View();
        }

       
        [Authorize(Roles = "Admin")]
        public IActionResult UserManagement()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateApprovalStatus(string userId, bool isApproved)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.IsApproved = isApproved;
                    await _userManager.UpdateAsync(user);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserRole(string userId, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, role);
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [HttpGet]
      
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.TCNo,
                    user.IsApproved,
                    ClassroomName = user.Classroom?.Name,
                    ClassroomId = user.ClassroomId,
                    roles
                });
            }

            return Json(userList);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserDetails(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            
            return Json(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.ClassroomId,
                user.IsApproved,
                Roles = roles
            });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetClassrooms()
        {
            var classrooms = await _classroomRepository.GetAllAsync();
            return Json(classrooms.Select(c => new { c.Id, c.Name }));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(string userId, int classroomId, string role, bool isApproved)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Json(new { success = false });

                // Sınıf güncelleme
                user.ClassroomId = classroomId;
                
                // Onay durumu güncelleme
                user.IsApproved = isApproved;

                // Kullanıcı güncelleme
                await _userManager.UpdateAsync(user);

                // Rol güncelleme
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, role);

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}
