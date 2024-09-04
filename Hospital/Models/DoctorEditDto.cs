namespace Hospital.Models
{
    public class DoctorEditDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int RoomId { get; set; }
        public int SpecializationId { get; set; }
        public int? SectionId { get; set; }
    }

    public class DoctorGet
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Room { get; set; }
        public string Specialization { get; set; }
        public int? Section { get; set; }
    }
}
