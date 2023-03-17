using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User;

namespace SCCPP1.Pages
{
    public class CreateMainProfileModel : SessionModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public CreateMainProfileModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger) : base(sessionHandler)
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

        // When the Submit button on the form is pressed on, OnPost() starts, grabs the information the user typed and then saves it into the database.
        // If it is successfully saved into the database, redirect the user to "/UserHome", if it fails, return this page.
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Colleague != null)
            {
                Account.Name = $"{Colleague.LastName}, {Colleague.FirstName} {Colleague.MiddleName?.ToString()} ";
                Account.EmailAddress = Colleague.EmailAddress;
                Account.PhoneNumber = Utilities.ParsePhoneNumber(Colleague.PhoneNumber);
                Account.IntroNarrative = Colleague.IntroNarrative;

                if (DatabaseConnector.SaveUser(Account))
                {
                    //Maybe have save and continue?
                    Console.WriteLine("Saved");
                    ViewData["UserData"] = "Saved!";
                    return RedirectToPage("/UserHome");
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