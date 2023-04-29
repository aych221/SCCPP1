using System.ComponentModel.DataAnnotations;
using SCCPP1.User.Data;

namespace SCCPP1.Models
{
    // Here we have our Certification class for model binding, with the getters and setters for the "Certification" section of the Account
    // ie. Institution, Certificate, and Dates etc.

    public class Certification
    {
        public int ID { get; set; }

        public string Value { get; set; }

        public string? Institution { get; set; }

        public string? Certificate { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}