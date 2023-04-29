using SCCPP1.User.Data;
using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here we have our Work class for model binding, with the getters and setters for the "Experience" section of the Account
    // ie. Experience, Employer, Job Title, Dates etc.
    // Description and Location is not used, but could be used for future ititerations of this project

    public class Work
    {
        public int ID { get; set; }

        public string? Experience { get; set; }

        public string? Employer { get; set; }

        public string? JobTitle { get ; set; }

        public string? Description { get; set; }

        public Location? Location { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}