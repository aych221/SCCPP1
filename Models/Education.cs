using System.ComponentModel.DataAnnotations;
using SCCPP1.User.Data;

namespace SCCPP1.Models
{
    // Here we have our Education class, with the getters and setters for the "Education" section of the Account
    // ie. the title of their degree/certification and the corresponding year of completion
    public class Education
    {
        public string? EduCert { get; set; }

        public string? EduYear { get; set; }

        public string? Study { get; set; }

        public Location? Location { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}
