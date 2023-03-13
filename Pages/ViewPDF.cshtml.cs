using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Models;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
	public class ViewPDFModel : SessionModel
    {

        private readonly ILogger<ViewPDFModel> _logger;

        public ViewPDFModel(SessionHandler sessionHandler, ILogger<ViewPDFModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }



        [BindProperty]
        public Colleague? Colleague { get; set; }

        public IActionResult OnGet()
        {
            Console.WriteLine("EditMainProfile.OnGet() Called");
            //invalid model state or the account is new
            if (!ModelState.IsValid)
                return Page();

            //TODO: add name fields in DB
            string[] names = Utilities.SplitFullName(Account.Name);


            ViewData["FullName"] = $"{names[0]}, {names[1]}";

            //<br /> does not work
            ViewData["Contact"] = $"Email: {Account.Email}<br>Phone: {Account.Phone}";

            ViewData["Intro"] = Account.IntroNarrative;

            
            StringBuilder sb = new StringBuilder();
            //edu history
            foreach (string s in DatabaseConnector.GetRawColleagueEducationHistory(Account.RecordID))
                sb.Append($"{s}<br>");

            ViewData["Education"] = sb.ToString();

            //clear for next use
            sb.Clear();
            

            //work history
            foreach (string s in DatabaseConnector.GetRawColleagueWorkHistory(Account.RecordID))
                sb.Append($"{s}<br>");

            ViewData["Work"] = sb.ToString();
            sb.Clear();


            //skills
            foreach (string s in DatabaseConnector.GetRawColleagueSkills(Account))
                sb.Append($"{s}<br>");
            ViewData["Skills"] = sb.ToString();


            return Page();
        }

        //TODO: should probably make EditMain and CreateMain same page and just change name.
        public IActionResult OnPost()
        {
            Console.WriteLine("EditMainProfile.OnPost() Called");
            if (!ModelState.IsValid)
                return Page();

            if (Colleague != null)
            {
                Account.Name = $"{Colleague.LastName}, {Colleague.FirstName} {Colleague.MiddleName?.ToString()}";
                Account.Email = Colleague.EmailAddress;
                Account.Phone = Utilities.ParsePhoneNumber(Colleague.PhoneNumber);
                Account.IntroNarrative = Colleague.IntroNarrative;

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