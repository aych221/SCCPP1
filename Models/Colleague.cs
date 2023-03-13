using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    public class Colleague
    {
        public int Id { get; set; }

        [Required]
        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        public string? LastName { get; set; }

        //maybe change to int
        [Required]
        public string? PhoneNumber { get; set; }

        [Required]
        public string? EmailAddress { get; set; }

        public string? IntroNarrative { get; set; }
    }
}