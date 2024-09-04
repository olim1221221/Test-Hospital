using Hospital.Data;
using Hospital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Получение списка пациентов с поддержкой сортировки и постраничного возврата
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientListDto>>> GetPatients([FromQuery] int page , [FromQuery] int pageSize , [FromQuery] string sortBy )
        {
            var patientsQuery = _context.Patients
                .Include(p => p.Section)
                .Select(p => new PatientListDto
                {
                    Id = p.Id,
                    FullName = $"{p.LastName} {p.FirstName} {p.MiddleName}",
                    Address = p.Address,
                    BirthDate = p.BirthDate,
                    Gender = p.Gender,
                    SectionName = p.Section.Number.ToString()
                });

            switch (sortBy.ToLower())
            {
                case "lastname":
                    patientsQuery = patientsQuery.OrderBy(p => p.FullName);
                    break;
                case "birthdate":
                    patientsQuery = patientsQuery.OrderBy(p => p.BirthDate);
                    break;
                default:
                    patientsQuery = patientsQuery.OrderBy(p => p.FullName);
                    break;
            }

            var patients = await patientsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(patients);
        }

        // Получение пациента по Id для редактирования
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientEditDto>> GetPatient(int id)
        {
            var patient = await _context.Patients
                .Where(p => p.Id == id)
                .Select(p => new PatientEditDto
                {
                    Id = p.Id,
                    LastName = p.LastName,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    Address = p.Address,
                    BirthDate = p.BirthDate,
                    Gender = p.Gender,
                    SectionId = p.Section.Number
                })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }

        // Создание нового пациента
        [HttpPost]
        public async Task<ActionResult<Patient>> CreatePatient(PatientEditDto patientDto)
        {
            var sections = await _context.Sections.FindAsync(patientDto.SectionId);
            if (sections == null)
            {
                return BadRequest("Sections doesn't exist.");
            }

            var patient = new Patient
            {
                LastName = patientDto.LastName,
                FirstName = patientDto.FirstName,
                MiddleName = patientDto.MiddleName,
                Address = patientDto.Address,
                BirthDate = patientDto.BirthDate,
                Gender = patientDto.Gender,
                SectionId = patientDto.SectionId
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
        }

        // Обновление данных пациента
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, PatientEditDto patientDto)
        {

            var sections = await _context.Sections.FindAsync(patientDto.SectionId);
            if (sections == null)
            {
                return BadRequest("Sections doesn't exist.");
            }


            if (id != patientDto.Id)
            {
                return BadRequest();
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            patient.LastName = patientDto.LastName;
            patient.FirstName = patientDto.FirstName;
            patient.MiddleName = patientDto.MiddleName;
            patient.Address = patientDto.Address;
            patient.BirthDate = patientDto.BirthDate;
            patient.Gender = patientDto.Gender;
            patient.SectionId = patientDto.SectionId;

            await _context.SaveChangesAsync();

            return Ok ();
        }

        // Удаление пациента
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
