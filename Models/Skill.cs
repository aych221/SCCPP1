using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here we have our Skill class, with the getters and setters for the "Skill" section of the Account
    // ie. languages known, operating system, software, and framework
    public class Skill
    {
        public int ID { get; set; }

        public string? ProgLang { get; set; }

        public string? OS { get; set; }

        public string? SoftAndFrame { get; set; }

        public string? Other { get; set; }
    }
}
