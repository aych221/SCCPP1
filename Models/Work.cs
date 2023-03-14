using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here we have our Work class, with the getters and setters for the "Experioence" section of the Colleague
    // Consideration is still being put in how we would like to format Experience
    // Ex. Do we want to save Experience as ExperienceTitle, ExperienceYear, and ExperienceDescription
    public class Work
    {
        public string? Experience { get; set; }
    }
}