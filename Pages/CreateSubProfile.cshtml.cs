using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User;
using SCCPP1.User.Data;

namespace SCCPP1.Pages
{
    public class CreateSubProfileModel : SessionModel
    {
        private readonly ILogger<PrivacyModel> _logger;


        //The following binds allow the selection result to be recorded

        [BindProperty]
        public string name { get; set; }
        [BindProperty]
        public string phone { get; set; }
        [BindProperty]
        public string email { get; set; }
        [BindProperty]
        public string introNar { get; set; }
        [BindProperty]
        public List<string> eduHist { get; set; }
        [BindProperty]
        public List<string> workHist { get; set; }
        [BindProperty]
        public List<string> skills { get; set; }
        [BindProperty]
        public List<string> certs { get; set; }



        //used for determining whether a selction is selected
        private string nameCheck, phoneCheck, emailCheck, introCheck, eduCheck, workCheck, skillCheck, certCheck;



        //Holds the HTML strings for multi-part (List) sections
        private string eduCollection;
        private string workCollection;
        private string skillsCollection;
        private string certCollection;

        //holds the currently selected profile
        private ProfileData p;


        public CreateSubProfileModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger) : base(sessionHandler)
        {
            Console.WriteLine("[CreateSubProfileModel] constructor called");
            _logger = logger;
        }


        ~CreateSubProfileModel()
        {
            Console.WriteLine("[CreateSubProfileModel] destructor called");
        }








        public IActionResult OnGet()
        {
            

            //imports chosen profile
            p = Account.ChosenProfile();
          

                // ViewData["subTitle"] = $"<input type = \"text\" placeholder = \"Name this profile\" name=\"subName\" id=\"subName\" value = \"{subName}\" readonly >";
                ViewData["subTitle"] = $"<b>{p.Title}</b><br>";

                //Displays the initial checkboxes, including all that have been used. Initially all Personal information is checked
                ViewData["name"] = $"<input type=\"checkbox\" id=\"name\" name=\"name\" onChange = \"this.form.submit()\" Checked>" +
                    $"<label for=\"about\"> Name </label><br>";

                ViewData["phone"] = $"<input type=\"checkbox\" id=\"phone\" name=\"phone\" onChange = \"this.form.submit()\" Checked> " +
                    $"<label for=\"about\"> Phone Number </label><br>";

                ViewData["email"] = $"<input type=\"checkbox\" id=\"email\" name=\"email\" onChange = \"this.form.submit()\" Checked>" +
                    $"<label for=\"about\"> Email </label><br>";

                ViewData["introNar"] = $"<input type=\"checkbox\" id=\"introNar\" name=\"introNar\" onChange = \"this.form.submit()\" Checked>" +
        $"<label for=\"about\"> Introduction Narrative </label><br>";




            /* The following will check all education data stored in this account, check to see if that record is part of the profile, and if it is checks it. 
            It also generates checkboxes for each, and if they are checked, adds that information to the "PDF"
            */
            string edString = "";
                eduCollection = "";

                foreach (EducationData e in Account.SavedEducationHistory.Values) //cycles through all saved data
                {
                    if (p.SelectedEducationIDs.Contains(e.RecordID)) //checks if this item is part of profile
                        eduCheck = "Checked";

                    //adds a checkbox
                    edString += $"<input type=\"checkbox\" name=\"eduHist\" value=\"{e.Institution}\" onChange = \"this.form.submit()\" {eduCheck} >" +
                    $"<label for=\"about\"> {e.Institution} </label><br>";
                
                    //if checked, adds the info to PDF
                    if (eduCheck == "Checked")
                    {
                        eduCollection += $"<br> {e.Institution}<br>{e.StartDate} to {e.EndDate}<br>{e.Description}<br>";
                    }

                    eduCheck = "";
                }

                //This is to add the category only if something in it has been selected
                if (eduCollection != "")
                {
                    eduCollection = $"<div class=\"education\"><h5><i class=\"fa fa-graduation-cap\"></i><b> Education </b></h5><p id=\"blue\">{eduCollection}</p></div>";
                }


            ViewData["edu"] = edString;


            //Same as Education, but for work
            string workString = "";

                workCollection = "";

                foreach (WorkData w in Account.SavedWorkHistory.Values)
                {
                    if (p.SelectedWorkIDs.Contains(w.RecordID))
                        workCheck = "Checked";
                    workString += $"<input type=\"checkbox\" name=\"workHist\" value=\"{w.Employer}\" onChange = \"this.form.submit()\" {workCheck}>" +
                     $"<label for=\"about\"> {w.Employer} </label><br>";

                    if (workCheck == "Checked")
                    {
                        workCollection += $"<br>{w.JobTitle}<br>{w.Employer} {w.StartDate} to {w.EndDate}<br>{w.Description}";
                    }

                    workCheck = "";
                }


                if (workCollection != "")
                {
                    workCollection = $"<div class=\"experience\"> <h5><i class=\"fa fa-briefcase\"></i><b> Experience </b></h5> <p id=\"red\">{workCollection}</p></div>";
                }

                ViewData["work"] = workString;



                //Same as above for skills
                string skillString = "";
                skillsCollection = "";

                foreach (SkillData s in Account.SavedSkills.Values)
                {
                    if (p.SelectedSkillIDs.Contains(s.RecordID))
                        skillCheck = "Checked";
                    skillString += $"<input type=\"checkbox\" name=\"skills\" value=\"{s.SkillName}\" onChange = \"this.form.submit()\" {skillCheck}>" +
                   $"<label for=\"about\"> {s.SkillName} </label><br>";

                    if (skillCheck == "Checked")
                    {
                        skillsCollection += $"<br>{s.SkillName}";
                    }

                    skillCheck = "";
                }

                if (skillsCollection != "")
                {
                    skillsCollection = $"<div class=\"skill\"><h5><i class=\"fa fa-lightbulb-o\"></i><b> Skills </b></h5><p id=\"blue\">{skillsCollection}</p></div>";
                }

                ViewData["skills"] = skillString;


            

                //Same as above, for Certs
                string certString = "";
                certCollection = "";

                foreach (CertificationData c in Account.SavedCertifications.Values)
                {
                    if (p.SelectedCertificationIDs.Contains(c.RecordID))
                        certCheck = "Checked";
                    certString += $"<input type=\"checkbox\" name=\"certs\" value=\"{c.CertificationType}\" onChange = \"this.form.submit()\" >" +
                       $"<label for=\"about\"> {c.CertificationType} </label><br>";

                    if (certCheck == "Checked")
                    {
                        certCollection += $"<br> {c.Institution}<br>{c.CertificationType} <br> {c.StartDate} to {c.EndDate}<br>";
                    }



                    certCheck = "";
                }

                if (certCollection != "")
                {
                    certCollection = $"<div class=\"education\"><h5><i class=\"fa fa-graduation-cap\"></i><b> Certiications </b></h5><p id=\"blue\">{certCollection}</p></div>";
                }


                ViewData["certs"] = certString;
            


                //These ViewDatas add the selected information to the PDF section of HTML
                ViewData["nameDisplay"] = $"<h1><b> {Account.Name} </b></h1>";
                ViewData["phoneDisplay"] = $" <h6><i class=\"fa fa-phone\"></i> {Account.PhoneNumber} </h6>";
                ViewData["emailDisplay"] = $"<h6><i class=\"fa fa-envelope\"></i> {Account.EmailAddress}</h6>";
                ViewData["introNarDisplay"] = $"<div id=\"about\"><h5><i class=\"fa fa-user\"></i><b> About </b> </h5> <p id=\"red\">{Account.IntroNarrative}</p></div>";
                ViewData["eduDisplay"] = eduCollection;
                ViewData["workDisplay"] = workCollection;
                ViewData["skillDisplay"] = skillsCollection;
                ViewData["certDisplay"] = certCollection;

            return Page();


        }







        public IActionResult OnPost()
        {
            p = Account.ChosenProfile();
            ViewData["subTitle"] = $"<b>{p.Title}</b><br>";


            nameCheck = (name == "on") ? "checked" : "";
            phoneCheck = (phone == "on") ? "checked" : "";
            emailCheck = (email == "on") ? "checked" : "";
            introCheck = (introNar == "on") ? "checked" : "";


            ViewData["name"] = $"<input type=\"checkbox\" id=\"name\" name=\"name\" onChange = \"this.form.submit()\" {nameCheck} >" +
                $"<label> Name </label><br>";

            ViewData["phone"] = $"<input type=\"checkbox\" id=\"phone\" name=\"phone\" onChange = \"this.form.submit()\" {phoneCheck}> " +
                $"<label> Phone Number </label><br>";

            ViewData["email"] = $"<input type=\"checkbox\" id=\"email\" name=\"email\" onChange = \"this.form.submit()\" {emailCheck}>" +
                $"<label> Email </label><br>";

            ViewData["introNar"] = $"<input type=\"checkbox\" id=\"introNar\" name=\"introNar\" onChange = \"this.form.submit()\" {introCheck}>" +
    $"<label for=\"about\"> Introduction Narrative </label><br>";




            //These are the same as in OnGet, with the addition of a check to see if it is currently checked

            string edString = "";
            eduCollection = "";
            foreach (EducationData e in Account.SavedEducationHistory.Values)
            {
                eduCheck = "";
                foreach (string ed in eduHist) //This loop cycles through all of the responses from the Post, and checks to see if any match, and if so, ensure they stay checked
                {
                    if (e.Institution == ed)
                    {
                        eduCheck = "checked";
                        p.AddEducation(e.RecordID);
                        break;
                    }
                }


                edString += $"<input type=\"checkbox\" name =\"eduHist\" value=\"{e.Institution}\" onChange = \"this.form.submit()\" {eduCheck}>" +
                $"<label for=\"about\"> {e.Institution} </label><br>";
               
                
                
                if (!eduCheck.Equals("checked"))
                {
                    p.RemoveEducation(e.RecordID);
                }


                if (eduCheck == "checked")
                {
                    eduCollection += $"<br> {e.Institution}<br>{e.StartDate} to {e.EndDate}<br>{e.Description}<br>";
                }
            }

            ViewData["edu"] = edString;
            if (eduCollection != "")
            {
                eduCollection = $"<div class=\"education\"><h5><i class=\"fa fa-graduation-cap\"></i><b> Education </b></h5><p id=\"blue\">{eduCollection}</p></div>";
            }






            string workString = "";
            workCollection = "";

            foreach (WorkData w in Account.SavedWorkHistory.Values)
            {

                workCheck = "";
                foreach (string work in workHist)
                {
                    if (w.Employer == work)
                    {
                        workCheck = "checked";
                        p.AddWork(w.RecordID);
                        break;
                    }
                }

                if (!workCheck.Equals("checked"))
                {
                    p.RemoveWork(w.RecordID);
                }


                workString += $"<input type=\"checkbox\" name=\"workHist\" value=\"{w.Employer}\" onChange = \"this.form.submit()\" {workCheck}>" +
                 $"<label for=\"about\"> {w.Employer} </label><br>";

                if (workCheck == "checked")
                {
                    workCollection += $"<br>{w.JobTitle}<br>{w.Employer} {w.StartDate} to {w.EndDate}<br>{w.Description}";
                }
            }

            if (workCollection != "")
            {
                workCollection = $"<div class=\"experience\"> <h5><i class=\"fa fa-briefcase\"></i><b> Experience </b></h5> <p id=\"red\">{workCollection}</p></div>";
            }
            ViewData["work"] = workString;







            string skillString = "";
            skillsCollection = "";

            foreach (SkillData s in Account.SavedSkills.Values)
            {
                skillCheck = "";
                foreach (string sk in skills)
                {
                    if (s.SkillName == sk)
                    {
                        skillCheck = "checked";
                        p.AddSkill(s.RecordID);
                        break;
                    }
                }

                skillString += $"<input type=\"checkbox\" name=\"skills\" value=\"{s.SkillName}\" onChange = \"this.form.submit()\" {skillCheck} >" +
               $"<label for=\"about\"> {s.SkillName} </label><br>";

                if (!skillCheck.Equals("checked"))
                {
                    p.RemoveSkill(s.RecordID);
                }

                if (skillCheck == "checked")
                {
                    skillsCollection += $"<br>{s.SkillName}";
                }
            }

            ViewData["skills"] = skillString;

            if (skillsCollection != "")
            {
                skillsCollection = $"<div class=\"skill\"><h5><i class=\"fa fa-lightbulb-o\"></i><b> Skills </b></h5><p id=\"blue\">{skillsCollection}</p></div>";
            }









            string certString = "";
            certCollection = "";

            foreach (CertificationData c in Account.SavedCertifications.Values)
            {
                certCheck = "";

                foreach (string ch in certs)
                {
                    if (c.CertificationType == ch)
                    {
                        certCheck = "Checked";
                        p.AddCertification(c.RecordID);
                        break;
                    }
                }

                certString += $"<input type=\"checkbox\" name=\"certs\" value=\"{c.CertificationType}\" onChange = \"this.form.submit()\" {certCheck} >" +
                   $"<label for=\"about\"> {c.CertificationType} </label><br>";

                if (!certCheck.Equals("checked"))
                {
                    p.RemoveCertification(c.RecordID);
                }
                if (certCheck == "Checked")
                {
                    certCollection += $"<br> {c.Institution}<br>{c.CertificationType} <br> {c.StartDate} to {c.EndDate}<br>";
                }



                if (certCollection != "")
                {
                    certCollection = $"<div class=\"education\"><h5><i class=\"fa fa-graduation-cap\"></i><b> Certiications </b></h5><p id=\"blue\">{certCollection}</p></div>";
                }
            }


            ViewData["certs"] = certString;




            if (name == "on")
                ViewData["nameDisplay"] = $"<h1><b> {Account.Name} </b></h1>";
            if (phone == "on")
                ViewData["phoneDisplay"] = $" <h6><i class=\"fa fa-phone\"></i> {Account.PhoneNumber} </h6>";
            if (email == "on")
                ViewData["emailDisplay"] = $"<h6><i class=\"fa fa-envelope\"></i> {Account.EmailAddress}</h6>";
            if (introNar == "on")
                ViewData["introNarDisplay"] = $"<div id=\"about\"><h5><i class=\"fa fa-user\"></i><b> About </b> </h5> <p id=\"red\">{Account.IntroNarrative}</p></div>";
            ViewData["eduDisplay"] = eduCollection;
            ViewData["workDisplay"] = workCollection;
            ViewData["skillDisplay"] = skillsCollection;
            ViewData["certDisplay"] = certCollection;

            p.Save();
            Account.PersistAll();
            

            return Page();
        }


/*
        [BindProperty]
        public List<CheckBoxViewModel> Skills { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> EducationHistory { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> Certifications { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> WorkHistory { get; set; }



        public IActionResult OnGet()
        {


            //imports chosen profile

            if (Account.ChosenProfile() == null)
            {
                Account.ChooseProfile(Account.CreateProfile(""));
                p = Account.ChosenProfile();
            }

            List<CheckBoxViewModel> LoadModel<T>(ReadOnlyDictionary<int, T> dict) where T : RecordData
            {
                return dict.Select(v => new CheckBoxViewModel
                {
                    RecordID = v.Value.RecordID,
                    IsSelected = true
                }).ToList();
            }

            Skills = LoadModel(Account.SavedSkills);

            EducationHistory = LoadModel(Account.SavedEducationHistory);

            Certifications = LoadModel(Account.SavedCertifications);

            WorkHistory = LoadModel(Account.SavedWorkHistory);





            return Page();
        }

        public IActionResult OnPost(List<CheckBoxViewModel> Skills, List<CheckBoxViewModel> EducationHistory, List<CheckBoxViewModel> Certifications, List<CheckBoxViewModel> WorkHistory)
        {
            // Handle form submission
            if (!ModelState.IsValid)
            {
                return Page();
            }

            foreach (var v in Skills)
            {
                if (v.IsSelected)
                {
                    Console.WriteLine($"Skill {v.RecordID} is selected");
                }
            }

            foreach (var v in EducationHistory)
            {
                if (v.IsSelected)
                {
                    Console.WriteLine($"EDU {v.RecordID} is selected");
                }
            }

            foreach (var v in Certifications)
            {
                if (v.IsSelected)
                {
                    Console.WriteLine($"Cert {v.RecordID} is selected");
                }
            }

            foreach (var v in WorkHistory)
            {
                if (v.IsSelected)
                {
                    Console.WriteLine($"WORK {v.RecordID} is selected");
                }
            }

            p.Save();
            Account.PersistAll();

            return Page();
        }
//*/
    }
}
