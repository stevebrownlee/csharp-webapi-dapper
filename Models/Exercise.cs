using System.Collections.Generic;

namespace NSSWeb.Data
{
    public class Exercise
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public List<Student> AssignedStudents { get; set; } = new List<Student>();
    }
}