using Microsoft.AspNetCore.Mvc;
using OnlineSinav.Models;
using OnlineSinav.Repositories;

namespace OnlineSinav.Controllers
{
    public class SinifController : Controller
    {
        private readonly ClassroomRepository _classroomRepository;

        public SinifController(ClassroomRepository classroomRepository)
        {
            _classroomRepository = classroomRepository;
        }

        public async Task<IActionResult> Index()
        {
            var classrooms = await _classroomRepository.GetAllAsync();
            return View(classrooms);
        }
        public async Task<IActionResult> GetAll()
        {
            var classrooms = await _classroomRepository.GetAllAsync();
            return Ok(classrooms);
        }


        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Classroom classroom)
        {
            await _classroomRepository.AddAsync(classroom);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, [FromBody] Classroom classroom)
        {
            if (id != classroom.Id)
            {
                return BadRequest();
            }
            await _classroomRepository.UpdateAsync(classroom);
            return Ok();
        }




        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _classroomRepository.DeleteAsync(id);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var classroom = await _classroomRepository.GetByIdAsync(id);
            if (classroom == null)
            {
                return NotFound();
            }
            return Ok(classroom);
        }
    }
}
