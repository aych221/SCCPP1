using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here we have our Skill class for model binding, with the getters and setters for the "Skill" section of the Account
    // ie. Programming Languages, Operating Systems, Software & Framework, Other etc.

    public class Skill
    {
        public int ID { get; set; }

        public string? ProgLang { get; set; }

        public string? OS { get; set; }

        public string? SoftAndFrame { get; set; }

        public string? Other { get; set; }
    }
}
