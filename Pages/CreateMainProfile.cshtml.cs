using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
    [BindProperties]
    public class CreateMainProfileModel : SessionModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public CreateMainProfileModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Person person { get; set; }

        public void OnPost()
        {
            ViewData["UserData"] = "User Information: "
                + person.firstname + " " + person.middlename + " " + person.lastname
                + person.phonenumber
                + person.eaddress
                + person.about
                + person.experience
                + person.complanguages
                + person.operatingsystems
                + person.softandframe
                + person.educationcertification
                + person.educationyear;
        }

        public class Person
        {
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string middlename { get; set; }
            public string phonenumber { get; set; }
            public string eaddress { get; set; }
            public string about { get; set; }
            public string experience { get; set; }
            public string complanguages { get; set; }
            public string operatingsystems { get; set; }
            public string softandframe { get; set; }
            public string educationcertification { get; set; }
            public string educationyear { get; set; }
        }
    }
}