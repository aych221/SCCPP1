using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here we have our Education class, with the getters and setters for the "Education" section of the Colleague
    // ie. the title of their degree/certification and the corresponding year of completion
    public class Education
    {
        public string? EduCert { get; set; }

        public string? EduYear { get; set; }
    }
}
