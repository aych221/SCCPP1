using Microsoft.AspNetCore.Mvc;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User;

namespace SCCPP1.Pages
{
    public class EditMainProfileModel : SessionModel
    {
        private readonly ILogger<EditMainProfileModel> _logger;

        public EditMainProfileModel(SessionHandler sessionHandler, ILogger<EditMainProfileModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }

        // The [BindProperty] attribute, takes the data from the POST request and maps it to our Model which can then be intereacted with our database.
        [BindProperty]
        public Colleague? Colleague { get; set; }

        [BindProperty]
        public Work? Work { get; set; }

        [BindProperty]
        public Skill? Skill { get; set; }

        [BindProperty]
        public Education? Education { get; set; }

        // When this page is initialize OnGet() starts before the instance of the user is created, therefore within our OnGet(),
        // We create a instance of the user and pull the saved data from the database to be displayed.
        public IActionResult OnGet()
        {
            Console.WriteLine("EditMainProfile.OnGet() Called");
            // invalid model state or the account is new
            if (!ModelState.IsValid || !Account.IsReturning)
                return Page();

            if (Colleague == null)
                Colleague = new Colleague();

            Console.WriteLine("Model State is valid and Account is returning");
            // TODO: add name fields in DB
            string[] names = Utilities.SplitFullName(Account.Name);
            Colleague.FirstName = names[1];
            Colleague.LastName= names[0];
            Colleague.MiddleName = names[2];
            Colleague.PhoneNumber = Account.PhoneNumber;
            Colleague.IntroNarrative = Account.IntroNarrative;
            Colleague.EmailAddress = Account.EmailAddress;

            //Education.ID = Account.EducationHistory[0].RecordID;

            return Page();
        }

        // When the Submit button on the form is pressed on, OnPost() starts, grabs the information the user typed and then saves it into the database.
        // If it is successfully saved into the database, redirect the user to "/UserHome", if it fails, return this page.
        // Currently, we have the user stay on this page after they, Submit, but for testing purposes we are leaving it on this page.
        // We are also considering making CreateMainProfile and EditMainProfile as just one page, but that is still to be decided.
        public IActionResult OnPost()
        {
            Console.WriteLine("EditMainProfile.OnPost() Called");
            if (!ModelState.IsValid)
                return Page();

            if (Colleague != null)
            {
                Account.Name = $"{Colleague.LastName}, {Colleague.FirstName} {Colleague.MiddleName?.ToString()}";
                Account.EmailAddress = Colleague.EmailAddress;
                Account.PhoneNumber = Colleague.PhoneNumber;
                Account.IntroNarrative = Colleague.IntroNarrative;

                //Account.AddSkills(
                //    Skill.ProgLang,
                //    Skill.OS,
                //    Skill.SoftAndFrame
                //    );

                //Account.AddWork(
                //    Work.Employer,
                //    Work.JobTitle,
                //    Work.Description,
                //    Work.Location,
                //    Work.StartDate,
                //    Work.EndDate
                //    );

                //Account.AddEducation(
                //    Education.EduCert,
                //    Education.EduType,
                //    Education.Study,
                //    Education.Location,
                //    Education.StartDate,
                //    Education.EndDate
                //    );

                if (DatabaseConnector.SaveUser(Account))
                {
                    Console.WriteLine("Saved");
                    ViewData["UserData"] = "Saved!";
                }
                else
                {
                    Console.WriteLine("Could not save");
                    ViewData["UserData"] = "Error Saving";
                }
            }

            return Page();
        }
    }
}