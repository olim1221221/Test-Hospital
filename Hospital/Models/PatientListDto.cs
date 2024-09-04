namespace Hospital.Models
{
    public class PatientListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string SectionName { get; set; }
    }
}
