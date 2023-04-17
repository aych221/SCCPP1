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



        // The [BindProperty] attribute, takes the data from the OnPost() request and maps it to our Models.
        // After which, it can then intereact with our database.
        [BindProperty]
        public Colleague? Colleague { get; set; }

        [BindProperty]
        public Work? Work { get; set; }

        [BindProperty]
        public Skill? Skill { get; set; }

        [BindProperty]
        public Education? Education { get; set; }

        [BindProperty]
        public Certification? Certification { get; set; }



        // Is called when submit button on the form is pressed. Saves the information into the database.
        // If successfully saved, redirect the user to "/UserHome", else, return to this page.
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Colleague != null)
            {
                // Save the "About" of the user
                Account.UpdateData(
                    Colleague.FirstName, 
                    Colleague.MiddleName, 
                    Colleague.LastName, 
                    Colleague.EmailAddress, 
                    Colleague.PhoneNumber, 
                    Colleague.IntroNarrative);

                // Print to console (checks to see if it saved)
                Console.WriteLine(Colleague.FirstName);
                Console.WriteLine(Colleague.MiddleName);
                Console.WriteLine(Colleague.LastName);
                Console.WriteLine(Colleague.EmailAddress);
                Console.WriteLine(Colleague.PhoneNumber);
                Console.WriteLine(Colleague.IntroNarrative);



                // Save the "Skills" of the user
                
                // Print to console (checks to see if it saved)



                // Save the "Experience" of the user

                // Print to console (checks to see if it saved)



                // Save the "Education" of the user

                // Print to console (checks to see if it saved)



                // Save the "Certification" of the user

                // Print to console (checks to see it it saved)







                //Account.Name = $"{Colleague.LastName}, {Colleague.FirstName} {Colleague.MiddleName?.ToString()} ";
                //Account.EmailAddress = Colleague.EmailAddress;
                //Account.PhoneNumber = Utilities.ParsePhoneNumber(Colleague.PhoneNumber);
                //Account.IntroNarrative = Colleague.IntroNarrative;
                //// Account.EducationHistory[0].Remove.RecordID;



                //Console.WriteLine(Skill.ProgLang);
                //Console.WriteLine(Skill.OS);
                //Console.WriteLine(Skill.SoftAndFrame);

                //Account.AddSkills(
                //    Skill.ProgLang,
                //    Skill.OS,
                //    Skill.SoftAndFrame
                //    );
                //ProgLang.Split()
                // and then loop,
                // also check the info before going into account

                //Account.AddWork(
                //    Work.Employer,
                //    Work.JobTitle,
                //    Work.Description,
                //    Work.Location,
                //    Work.StartDate,
                //    Work.EndDate
                //    );

                //Account.AddEducation(
                //    Education.Institution,
                //    Education.Degree,
                //    Education.Field,
                //    Education.Location,
                //    Education.StartDate,
                //    Education.EndDate
                //    );

                //Account.AddCertification(
                //    Certification.Institution,
                //    Certification.Certificate,
                //    Certification.StartDate,
                //    Certification.EndDate
                //    );

                //if (DatabaseConnector.SaveUser(Account))
                //{
                //    //Maybe have save and continue?
                //    Console.WriteLine("Saved");
                //    ViewData["UserData"] = "Saved!";
                //    return RedirectToPage("/UserHome");
                //}
                //else
                //{
                //    Console.WriteLine("Could not save");
                //    ViewData["UserData"] = "Error Saving";
                //}

                Account.PersistAll();
            }
            return RedirectToPage("/UserHome");
        }
    }
}