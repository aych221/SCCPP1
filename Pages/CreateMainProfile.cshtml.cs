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

        // The [BindProperty] is used in ASP.NET Core for model binding.
        // This automates the process of passing information from an HTTP request into an input model.
        // In short, it tells our program to target binding on specific properties.
        [BindProperty]
        public Colleague? Colleague { get; set; }

        [BindProperty]
        public Skill? Skill { get; set; }

        [BindProperty]
        public Work? Work { get; set; }

        [BindProperty]
        public Education? Education { get; set; }

        [BindProperty]
        public Certification? Certification { get; set; }



        // OnPost() is called when submit button on the form is pressed. Saves the information into the database.
        // If successfully saved, redirect the user to "/UserHome", else, return to this page.
        public IActionResult OnPost()
        {
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
                // Programming Languages
                if (Skill.ProgLang != null)
                {
                    var plang = Request.Form["Skill.ProgLang"].ToString().Split(',');
                    Account.AddSkills("Programming Languages", plang);
                }
                // Print to console (checks to see if it saved)
                Console.WriteLine(Skill.ProgLang);

                // Operating Systems
                if (Skill.OS != null)
                {
                    var osys = Request.Form["Skill.OS"].ToString().Split(',');
                    Account.AddSkills("OS", osys);
                }
                // Print to console (checks to see if it saved)
                Console.WriteLine(Skill.OS);

                // Software & Framework
                if (Skill.SoftAndFrame != null)
                {
                    var snf = Request.Form["Skill.SoftAndFrame"].ToString().Split(',');
                    Account.AddSkills("Software and Framework", snf);
                }
                // Print to console (checks to see if it saved)
                Console.WriteLine(Skill.SoftAndFrame);

                // Other Skills
                if (Skill.Other != null)
                {
                    var other = Request.Form["Skill.Other"].ToString().Split(',');
                    Account.AddSkills("Other", other);
                }
                // Print to console (checks to see if it saved)
                Console.WriteLine(Skill.Other);



                // Save the "Experience" of the user
                string hiddenexp = Request.Form["Work.Experience"];
                if (hiddenexp != null)
                {
                    var experience = Request.Form["Work.Experience"].ToString().Split(';');
                    for (int i=0; i< experience.Length-1; i++) 
                    {
                        var exp = experience[i];
                        string[] e = exp.ToString().Split('|');
                        Account.AddWork(
                            e[0].ToString(), 
                            e[1].ToString(), 
                            null, 
                            null, 
                            Utilities.ToDateOnly(e[2].ToString()), 
                            Utilities.ToDateOnly(e[3].ToString()));
                    }
                }
                // Print to console (checks to see if it saved)
                Console.WriteLine(Work.Experience);


                // Save the "Education" of the user
                string hiddenedu = Request.Form["Education.Value"];
                if (hiddenedu != null)
                {
                    var education = Request.Form["Education.Value"].ToString().Split(';');
                    for (int i = 0; i < education.Length - 1; i++)
                    {
                        var edu = education[i];
                        string[] e = edu.ToString().Split('|');
                        Account.AddEducation(
                            e[0].ToString(),
                            e[1].ToString(),
                            e[2].ToString(),
                            null,
                            Utilities.ToDateOnly(e[3].ToString()),
                            Utilities.ToDateOnly(e[4].ToString()));
                    }
                }
                // Print to console (checks to see if it saved)
                Console.WriteLine(Education.Value);


                // Save the "Certification" of the user
                string hiddencert = Request.Form["Certification.Value"];
                if (hiddencert != null)
                {
                    var certification = Request.Form["Certification.Value"].ToString().Split(';');
                    for (int i = 0; i < certification.Length - 1; i++)
                    {
                        var cert = certification[i];
                        string[] e = cert.ToString().Split('|');
                        Account.AddCertification(
                            e[0].ToString(),
                            e[1].ToString(),
                            Utilities.ToDateOnly(e[2].ToString()),
                            Utilities.ToDateOnly(e[3].ToString()));
                    }
                }
                // Print to console (checks to see it it saved)
                Console.WriteLine(Certification.Value);

                Account.PersistAll();
            }
            return RedirectToPage("/UserHome");
        }
    }
}