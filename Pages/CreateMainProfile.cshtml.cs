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




        [BindProperty]
        public Colleague? Colleague { get; set; }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Colleague != null)
            {
                Account.Name = $"{Colleague.LastName}, {Colleague.FirstName} {Colleague.MiddleName?.ToString()} ";
                Account.Email = Colleague.EmailAddress;
                Account.Phone = Utilities.ParsePhoneNumber(Colleague.PhoneNumber);
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



        //ViewData["UserData"] = "User Information: ";
        /*     + person.firstname + " " + person.middlename + " " + person.lastname
             + person.phonenumber
             + person.eaddress
             + person.about
             + person.experience
             + person.complanguages
             + person.operatingsystems
             + person.softandframe
             + person.educationcertification
             + person.educationyear;*/
        /*
                [BindProperty]
                public Person person { get; set; }

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
                }*/
    }
}