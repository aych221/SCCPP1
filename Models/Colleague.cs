using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here we have our Colleague class for model binding, with the getters and setters for the "About" section of the Account
    // ie. Name, Phone, Email, Intro etc.

    public class Colleague
    {
        public int ID { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        //maybe change to int
        [Required]
        public long PhoneNumber { get; set; }

        [Required]
        public string EmailAddress { get; set; }

        public string? IntroNarrative { get; set; }
    }
}