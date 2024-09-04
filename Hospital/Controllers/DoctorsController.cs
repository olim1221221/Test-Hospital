using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получение списка врачей с поддержкой сортировки и постраничного возврата
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorListDto>>> GetDoctors([FromQuery] int page, [FromQuery] int pageSize , [FromQuery] string sortBy )
        {
            var doctorsQuery = _context.Doctors
                .Include(d => d.Room)
                .Include(d => d.Specialization)
                .Include(d => d.Section)
                .Select(d => new DoctorListDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    RoomNumber = d.Room.Number.ToString(),
                    SpecializationName = d.Specialization.Name,
                    SectionNumber = d.Section != null ? d.Section.Number.ToString() : "N/A"
                });

            switch (sortBy.ToLower())
            {
                case "fullname":
                    doctorsQuery = doctorsQuery.OrderBy(d => d.FullName);
                    break;
                case "specialization":
                    doctorsQuery = doctorsQuery.OrderBy(d => d.SpecializationName);
                    break;
                default:
                    doctorsQuery = doctorsQuery.OrderBy(d => d.FullName);
                    break;
            }

            var doctors = await doctorsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(doctors);
        }

        // Получение врача по Id для редактирования
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorGet>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Where(d => d.Id == id)
                .Select(d => new DoctorGet
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    Room = d.Room.Number ,        
                    Specialization = d.Specialization.Name,
                    Section = d.Section.Number
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return NotFound();
            }

            return Ok(doctor);
        }

        // Создание нового врача
        [HttpPost]
        public async Task<ActionResult<Doctor>> CreateDoctor(DoctorEditDto doctorDto)
        {
            var room = await _context.Rooms.FindAsync(doctorDto.RoomId);
            if (room == null)
            {
                return BadRequest("Room doesn't exist.");
            }

            // Проверка существования специализации
            var specialization = await _context.Specializations.FindAsync(doctorDto.SpecializationId);
            if (specialization == null)
            {
                return BadRequest("Specialization doesn't exist.");
            }

            // Проверка существования секции
            var section = await _context.Sections.FindAsync(doctorDto.SectionId);
            if (section == null)
            {
                return BadRequest("Section doesn't exist.");
            }

            var doctor = new Doctor
            {
                FullName = doctorDto.FullName,
                RoomId = doctorDto.RoomId,
                SpecializationId = doctorDto.SpecializationId,
                SectionId = doctorDto.SectionId
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, doctor);
        }

        // Обновление данных врача
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, DoctorEditDto doctorDto)
        {

            var room = await _context.Rooms.FindAsync(doctorDto.RoomId);
            if (room == null)
            {
                return BadRequest("Room doesn't exist.");
            }

            // Проверка существования специализации
            var specialization = await _context.Specializations.FindAsync(doctorDto.SpecializationId);
            if (specialization == null)
            {
                return BadRequest("Specialization doesn't exist.");
            }

            // Проверка существования секции
            var section = await _context.Sections.FindAsync(doctorDto.SectionId);
            if (section == null)
            {
                return BadRequest("Section doesn't exist.");
            }

            if (id != doctorDto.Id)
            {
                return BadRequest();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            doctor.FullName = doctorDto.FullName;
            doctor.RoomId = doctorDto.RoomId;
            doctor.SpecializationId = doctorDto.SpecializationId;
            doctor.SectionId = doctorDto.SectionId;

            await _context.SaveChangesAsync();

            return Ok ();
        }

        // Удаление врача
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok ();
        }
    }

}
