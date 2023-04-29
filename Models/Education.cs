using System.ComponentModel.DataAnnotations;
using SCCPP1.User.Data;

namespace SCCPP1.Models
{
    // Here we have our Education class for model binding, with the getters and setters for the "Education" section of the Account
    // ie. Institution, Degree, Field, Dates etc.
    // Location is not used, but could be used for future ititerations of this project

    public class Education
    {
        public int ID { get; set; }

        public string Value { get; set; }

        public string? Institution { get; set; }

        public string? Degree { get; set; }

        public string? Field { get; set; }

        public Location? Location { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}
