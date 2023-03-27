using SCCPP1.User.Data;
using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here we have our Work class, with the getters and setters for the "Experience" section of the Account
    // Consideration is still being put in how we would like to format Experience
    // Ex. Do we want to save Experience as ExperienceTitle, ExperienceYear, and ExperienceDescription
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