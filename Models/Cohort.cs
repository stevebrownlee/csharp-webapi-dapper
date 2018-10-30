using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercises.Data
{
    public class Cohort
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^([Dd]ay|[Ee]vening)\s[0-9]{1,2}$",
            ErrorMessage="Cohort name should be in the format of [Day|Evening] [number]")]
        public string Name { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Instructor> Instructors { get; set; } = new List<Instructor>();
    }

}