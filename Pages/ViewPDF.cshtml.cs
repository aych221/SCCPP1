using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User.Data;

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

            Console.WriteLine("ViewPDF.OnGet() Called");
            //invalid model state or the account is new
            if (!ModelState.IsValid)
                return Page();

            //TODO: add name fields in DB
            string[] names = Utilities.SplitFullName(Account.Name);


            ViewData["FullName"] = $"{names[0]}, {names[1]}";

            //<br /> does not work
            ViewData["Contact"] = $"Email: {Account.EmailAddress}<br>Phone: {Account.PhoneNumber}";

            ViewData["Intro"] = Account.IntroNarrative;

            
            StringBuilder sb = new StringBuilder();
            
            if (Account.LoadEducationHistory())
            {

                //edu history
                foreach (EducationData ed in Account.EducationHistory)
                {
                    sb.Append($"<p><b>{ed.Institution}</p></b>");
                    sb.Append($"<p>{ed.StartDate.ToString()}-{ed.EndDate.ToString()}<br>");
                    sb.Append($"{ed.EducationType} {ed.Description}</p><br><br>");
                    sb.Append($"=====================================================");
                }

                ViewData["Education"] = sb.ToString();

                //clear for next use
                sb.Clear();

            }
            
            if (Account.LoadWorkHistory())
            {

                //work history
                foreach (WorkData wd in Account.WorkHistory)
                {
                    sb.Append($"<p><b>{wd.JobTitle}</b> at {wd.Employer}</p>");
                    sb.Append($"<p>{wd.StartDate.ToString()}-{wd.EndDate.ToString()}<br>");
                    sb.Append($"{wd.Description}</p><br><br>");
                    sb.Append($"=====================================================");
                }

                ViewData["Work"] = sb.ToString();

                //clear for next use
                sb.Clear();
            }


            if (!Account.LoadSkills())
            {
                ViewData["Skills"] = "Failed to load skills";
                return Page();
            }
            Console.WriteLine("Skills: " + Account.Skills.Count);
            //skills
            foreach (SkillData sd in Account.Skills)
                sb.Append($"{sd.SkillName}<br>");
            ViewData["Skills"] = sb.ToString();

            // testing how data will load on front end
            StringBuilder sB = new StringBuilder();
            string title = "Master of Business Administration";
            string year = "2019";
            string institution = "University of Wisconsin";

            /*
            if (true)
            {
                // foreach (EducationData ed in Account.WorkHistory) {
                sB.Append($"<table>");
                sB.Append($"<tr>");
                sB.Append($"<b>{title}</b><br>");
                sB.Append($"{institution} | {year}");
                sB.Append($"</tr>");
                sB.Append($"</table>");
                sB.Append($"<br>");
                // }
            }
            ViewData["TestData"] = sB.ToString();
            */

            return Page();
        }

        //TODO: should probably make EditMain and CreateMain same page and just change name.
        public IActionResult OnPost()
        {
            Console.WriteLine("ViewPDF.OnPost() Called");
            if (!ModelState.IsValid)
                return Page();


            return Page();
        }
    }
}