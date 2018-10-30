using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StudentExercises.Data
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        [StringLength(25, MinimumLength=2)]
        public string LastName { get; set; }

        [Required]
        public string SlackHandle { get; set; }

        [Required]
        [Display(Name="Cohort")]

        public int CohortId { get; set; }

        public Cohort Cohort { get; set; }

        public List<Exercise> AssignedExercises { get; set; } = new List<Exercise>();
    }
}