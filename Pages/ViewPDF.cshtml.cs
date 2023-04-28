using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SCCPP1.Database.Entity;
using SCCPP1.Database.Sqlite;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User;
using SCCPP1.User.Data;

namespace SCCPP1.Pages
{

    public class ViewPDFModel : SessionModel
    {
        private readonly ILogger<ViewPDFModel> _logger;

        public ViewPDFModel(SessionHandler sessionHandler, ILogger<ViewPDFModel> logger) : base(sessionHandler)
        {
            _logger = logger;
            Console.WriteLine("[ViewPDFModel] constructor called");
        }


        //This section sets up the binding for the savable checkboxes
        [BindProperty]
        public string nameC { get; set; }
        [BindProperty]
        public string phoneC { get; set; }
        [BindProperty]
        public string emailC { get; set; }
        [BindProperty]
        public string introC { get; set; }
        [BindProperty]
        public List<CheckBoxViewModel> Skills { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> EducationHistory { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> Certifications { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> WorkHistory { get; set; }



        private ProfileData p;

        public IActionResult OnGet()  //on page load
        {
            //sets selected profile to one chosen on User Home

            p = Account.ChosenProfile();
            if (p is null) //If someone navigates directly to the page, i.e. does not select a profile, auto-generates one
                Account.ChooseProfile(Account.CreateProfile("ViewPDF Profile"));
            ViewData["profileName"] = p.Title;

            List<CheckBoxViewModel> LoadModel<T>(ReadOnlyDictionary<int, T> dict) where T : RecordData
            {
                return dict.Select(v => new CheckBoxViewModel
                {
                    RecordID = v.Value.RecordID,
                    IsSelected = true
                }).ToList();
            }

            //This section pre-loads the checkboxes

            Skills = Account.SavedSkills.Select(v => new CheckBoxViewModel
            {
                RecordID = v.Value.RecordID,
                IsSelected = p.SelectedSkillIDs.Contains(v.Value.RecordID)
            }).ToList();

            EducationHistory = Account.SavedEducationHistory.Select(v => new CheckBoxViewModel
            {
                RecordID = v.Value.RecordID,
                IsSelected = p.SelectedEducationIDs.Contains(v.Value.RecordID)
            }).ToList();

            Certifications = Account.SavedCertifications.Select(v => new CheckBoxViewModel
            {
                RecordID = v.Value.RecordID,
                IsSelected = p.SelectedCertificationIDs.Contains(v.Value.RecordID)
            }).ToList();

            WorkHistory = Account.SavedWorkHistory.Select(v => new CheckBoxViewModel
            {
                RecordID = v.Value.RecordID,
                IsSelected = p.SelectedWorkIDs.Contains(v.Value.RecordID)
            }).ToList();



            string progLangs = "";
            string opSys = "";
            string sF = "";
            string other = "";
            bool first = true;


            //These sections set the intial state of the About section in both the left checkbox and right PDF sections

            string name = "", nameDisplay;

            if (p.ShowName)
            {
                name = "<input type=\"checkbox\" id=\"nameC\" name=\"nameC\" onchange=\"hideName()\" checked>                  <label for=\"nameC\"><b>Colleague Name</b></label><br>";
                nameDisplay = $"<div id=\"name\" style=\"display:block\">      <h1><b>{Account.Name} </b></h1>          </div>";

            }
            else
            {
                name = "<input type=\"checkbox\" id=\"nameC\" name=\"nameC\" onchange=\"hideName()\" >                  <label for=\"nameC\"><b>Colleague Name</b></label><br>";
                nameDisplay = $"<div id=\"name\" style=\"display:none\">      <h1><b>{Account.Name} </b></h1>          </div>";
            }
            ViewData["name"] = name;
            ViewData["nameDisplay"] = nameDisplay;


            string phone = "", phoneDisplay = "";

            if (p.ShowPhoneNumber)
            {
                phone = "<input type=\"checkbox\" id=\"phoneC\" name=\"phoneC\" onchange=\"hidePhone()\" checked>                   <label for=\"phoneC\"><b>Colleague Phone</b></label><br>";
                phoneDisplay = $"<div id=\"phone\" style=\"display:block\">   | <i class=\"fa fa-phone\"></i>   {Account.PhoneNumber} |                  </div>";

            }
            else
            {
                phone = "<input type=\"checkbox\" id=\"phoneC\" name=\"phoneC\" onchange=\"hidePhone()\">                   <label for=\"phoneC\"><b>Colleague Phone</b></label><br>";
                phoneDisplay = $"<div id=\"phone\" style=\"display:none\">   | <i class=\"fa fa-phone\"></i>   {Account.PhoneNumber} |                  </div>";
            }
            ViewData["phone"] = phone;
            ViewData["phoneDisplay"] = phoneDisplay;

            string email = "", emailDisplay = "";

            if (p.ShowEmailAddress)
            {
                email = " <input type=\"checkbox\" id=\"emailC\" name=\"emailC\" onchange=\"hideEmail()\" checked>                  <label for=\"emailC\"><b>Colleague Email</b></label><br>";
                emailDisplay = $"<div id=\"email\" style=\"display:block\">                 | <i class=\"fa fa-envelope\"></i>   {Account.EmailAddress} |   </div>";

            }
            else
            {
                email = " <input type=\"checkbox\" id=\"emailC\" name=\"emailC\" onchange=\"hideEmail()\" >                  <label for=\"emailC\"><b>Colleague Email</b></label><br>";
                emailDisplay = $"<div id=\"email\" style=\"display:none\">                 | <i class=\"fa fa-envelope\"></i>   {Account.EmailAddress} |   </div>";
            }

            ViewData["email"] = email;
            ViewData["emailDisplay"] = emailDisplay;

            string intro = "", introDisplay = "";

            if (p.ShowIntroNarrative)
            {
                intro = "<input type=\"checkbox\" id=\"introC\" name=\"introC\" onchange=\"hideIntro()\" checked>\r\n                    <label for=\"introC\"><b>Intro Narrative</b></label><br>";
                introDisplay = $"<div id=\"intro\" style=\"display:block; text-align: left\">               <h5><i class=\"fa fa-user\"></i><b> About </b></h5>        <p id=\"red\"> {Account.IntroNarrative} </p>      </div>";
            }
            else
            { 
                intro = "<input type=\"checkbox\" id=\"introC\" name=\"introC\" onchange=\"hideIntro()\" >\r\n                    <label for=\"introC\"><b>Intro Narrative</b></label><br>";
                introDisplay = $"<div id=\"intro\" style=\"display:none; text-align: left\">               <h5><i class=\"fa fa-user\"></i><b> About </b></h5>        <p id=\"red\"> {Account.IntroNarrative} </p>      </div>";
            }
            ViewData["intro"] = intro;
            ViewData["introDisplay"] = introDisplay;


            //The following sections (one each for skills, edu, work, and certs) will iterate through all of the saved data, and assign them their condition in the HTML (namely, whether they are displayed or hidden via the Display: CSS)
            foreach (var v in Skills)
            {
                if (v.IsSelected)
                {
                    
                    if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Programming Languages")
                    {
                        progLangs += $"<li><div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "} </div></li> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "OS") 
                    {
                        opSys += $"<li><div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "}</div> </li> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Software and Framework")
                    {
                        sF += $"<li><div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "} </div> </li> ";

                    }
                    else
                    {
                        other += $"<li><div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "} </div> </li> ";

                    }
                }
                else
                {
                    if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Programming Languages")
                    {
                        progLangs += $"<li><div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "} </div> </li> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "OS")
                    {
                        opSys += $"<li><div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "} </div> </li> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Software and Framework")
                    {
                        sF += $"<li><div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "}</div> </li> ";

                    }
                    else
                    {
                        other += $"<li><div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName + " "}</div> </li> ";

                    }
                }
            }

            ViewData["progLangDisplay"] = progLangs;
            ViewData["osDisplay"] = opSys;
            ViewData["sfDisplay"] = sF;
            ViewData["otherDisplay"] = other;


            string edu = "";
            foreach (var v in EducationHistory)
            {

                //This subsection determines if a default date is used for an enddate (done when selecting "Present" in the CreateMain) and replaces it with Present
                string date;
                if (Account.GetEducationData(v.RecordID).EndDate.ToString().Equals("1/1/0001"))
                    date = "Present";
                else
                    date = Account.GetEducationData(v.RecordID).EndDate.ToString();

                if (!first)
                    edu += "<br>";
                first = false;
                if (v.IsSelected)
                {
                    Console.WriteLine($"EDU {v.RecordID} is selected");
                    edu += $"<div style = \"display:block\" id =\"edu_{v.RecordID}\"><b>" + Account.GetEducationData(v.RecordID).Institution + "</b><br><i>" + Account.GetEducationData(v.RecordID).EducationType + " in " + Account.GetEducationData(v.RecordID).Description + "</i><br>" + "From: " + Account.GetEducationData(v.RecordID).StartDate + "to " + date + "</div>";
                }
                else
                {
                    edu += $"<div style = \"display:none\" id =\"edu_{v.RecordID}\"><b>" + Account.GetEducationData(v.RecordID).Institution + "</b><br><i>" + Account.GetEducationData(v.RecordID).EducationType + " in " + Account.GetEducationData(v.RecordID).Description + " </i><br>" + "From: " + Account.GetEducationData(v.RecordID).StartDate + "to " + date + "</div>";
                }

            
            }
            ViewData["eduDisplay"] = edu;

            first = true;
            string certs = "";
            foreach (var v in Certifications)
            {
                if (!first)
                    certs += "<br>";
                first = false;
                if (v.IsSelected)
                {
                    Console.WriteLine($"Skill {v.RecordID} is selected");
                    certs += $"<div style = \"display:block\" id = \"certs_{v.RecordID}\"> {Account.GetCertificationData(v.RecordID).Institution} {Account.GetCertificationData(v.RecordID).CertificationType} </div>  ";
                }
                else
                {
                    certs += $"<div style = \"display:none\" id = \"certs_{v.RecordID}\"> {Account.GetCertificationData(v.RecordID).Institution} {Account.GetCertificationData(v.RecordID).CertificationType} </div>  ";
                }
            }
            ViewData["certDisplay"] = certs;


            first = true;
            string work = "";
            foreach (var v in WorkHistory)
            {
                if (!first)
                    work += "<br>";
                first = false;

                string date;                
                if (Account.GetWorkData(v.RecordID).EndDate.ToString().Equals("1/1/0001"))
                    date = "Present";
                else
                    date = Account.GetWorkData(v.RecordID).EndDate.ToString();

                if (v.IsSelected)
                {
                    Console.WriteLine($"Skill {v.RecordID} is selected");
                    work += $"<div style = \"display:block\" id = \"work_{v.RecordID}\"> <b>{Account.GetWorkData(v.RecordID).Employer}</b> <br> {Account.GetWorkData(v.RecordID).JobTitle} <br> {Account.GetWorkData(v.RecordID).StartDate} to {date} <br> {Account.GetWorkData(v.RecordID).Description}  </div> ";
                }
                else
                {
                    work += $"<div style = \"display:none\" id = \"work_{v.RecordID}\"> <b>{Account.GetWorkData(v.RecordID).Employer}</b> <br> {Account.GetWorkData(v.RecordID).JobTitle} <br> {Account.GetWorkData(v.RecordID).StartDate} to {date} <br> {Account.GetWorkData(v.RecordID).Description} </div> ";
                }
            } 
            ViewData["workDisplay"] = work;


            return Page();
        }

        public IActionResult OnPost(List<CheckBoxViewModel> Skills, List<CheckBoxViewModel> EducationHistory, List<CheckBoxViewModel> Certifications, List<CheckBoxViewModel> WorkHistory)
        {
            // Handle form submission
            //if (!ModelState.IsValid)
            //{
            //    Console.WriteLine("ERROR");
            //    return Page();
            //}

            p = Account.ChosenProfile();


            //saves the bool of whether these have been selected or not
            if(nameC == "on")
            {
                p.ShowName = true;
            }
            else
            {
                p.ShowName=false;
            }

            if (phoneC == "on")
            {
                p.ShowPhoneNumber = true;
            }
            else
            {
                p.ShowPhoneNumber = false;
            }

            if (emailC == "on")
            {
                p.ShowEmailAddress= true;
            }
            else
            {
                p.ShowEmailAddress = false;
            }
            if (introC == "on")
            {
                p.ShowIntroNarrative= true;
            }
            else
            {
                p.ShowIntroNarrative= false;
            }

            //Saves what has been selected, by either adding or removing the skill from the profile if has been checked or not.
            foreach (var s in Skills)
            {
                if (s.IsSelected)
                {

                    p.AddSkill(s.RecordID);
                }
                else
                {
                    p.RemoveSkill(s.RecordID);
                }
            }

            foreach (var e in EducationHistory)
            {
                if (e.IsSelected)
                {
                    p.AddEducation(e.RecordID);
                }
                else
                {
                    p.RemoveEducation(e.RecordID);
                }
            }

            foreach (var c in Certifications)
            {
                if (c.IsSelected)
                {
                    p.AddCertification(c.RecordID);
                }
                else
                {
                    p.RemoveCertification(c.RecordID);
                }
            }
            foreach (var w in WorkHistory)
            {
                if (w.IsSelected)
                {
                    p.AddWork(w.RecordID);
                }
                else
                {
                    p.RemoveWork(w.RecordID);
                }
            }

            p.Save();
            //Saves the profile
            Account.PersistAll();

            //The prsumption is they are done when they click the exit button, so dumps back to the Home page.
            return RedirectToPage("UserHome");

        }
    }
}

