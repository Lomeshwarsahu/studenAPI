using System.ComponentModel.DataAnnotations;

namespace tsting_api.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }

        public int Age { get; set; }
    }
}
