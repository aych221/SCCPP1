using System;
using System.Collections.Generic;
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
        [BindProperty]
        public string subName { get; set; }


        //used for determining whether a selction is selected
        private string nameCheck, phoneCheck, emailCheck, introCheck, eduCheck, workCheck, skillCheck, certCheck;



        //Holds the HTML strings for multi-part (List) sections
        private string eduCollection;
        private string workCollection;
        private string skillsCollection;
        private string certCollection;



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
            Account.PersistAll();

            if (subName == null)
            {
                ViewData["subTitle"] = $"<input type = \"text\" placeholder = \"Name this profile\" name=\"subName\" id=\"subName\" required />";

            }
            else
            {

                ViewData["subTitle"] = $"<input type = \"text\" placeholder = \"Name this profile\" name=\"subName\" id=\"subName\" value = \"{subName}\" readonly >";


                //Displays the initial checkboxes, including all that have been used
                ViewData["name"] = $"<input type=\"checkbox\" id=\"name\" name=\"name\" onChange = \"this.form.submit()\" >" +
                    $"<label for=\"about\"> Name </label><br>";

                ViewData["phone"] = $"<input type=\"checkbox\" id=\"phone\" name=\"phone\" onChange = \"this.form.submit()\"> " +
                    $"<label for=\"about\"> Phone Number </label><br>";

                ViewData["email"] = $"<input type=\"checkbox\" id=\"email\" name=\"email\" onChange = \"this.form.submit()\">" +
                    $"<label for=\"about\"> Email </label><br>";

                ViewData["introNar"] = $"<input type=\"checkbox\" id=\"introNar\" name=\"introNar\" onChange = \"this.form.submit()\">" +
        $"<label for=\"about\"> Introduction Narrative </label><br>";



                string edString = "";
                eduCollection = "";

                foreach (EducationData e in Account.SavedEducationHistory.Values)
                {
                    edString += $"<input type=\"checkbox\" name=\"eduHist\" value=\"{e.Institution}\" onChange = \"this.form.submit()\" >" +
                    $"<label for=\"about\"> {e.Institution} </label><br>";
                }

                string workString = "";
                ViewData["edu"] = edString;

                workCollection = "";

                foreach (WorkData w in Account.SavedWorkHistory.Values)
                {
                    workString += $"<input type=\"checkbox\" name=\"workHist\" value=\"{w.Employer}\" onChange = \"this.form.submit()\" >" +
                     $"<label for=\"about\"> {w.Employer} </label><br>";
                }

                ViewData["work"] = workString;


                string skillString = "";
                skillsCollection = "";

                foreach (SkillData s in Account.SavedSkills.Values)
                {
                    skillString += $"<input type=\"checkbox\" name=\"skills\" value=\"{s.SkillName}\" onChange = \"this.form.submit()\" >" +
                   $"<label for=\"about\"> {s.SkillName} </label><br>";
                }

                ViewData["skills"] = skillString;


                string certString = "";
                certCollection = "";

                foreach (CertificationData c in Account.SavedCertifications.Values)
                {
                    certString += $"<input type=\"checkbox\" name=\"certs\" value=\"{c.CertificationType}\" onChange = \"this.form.submit()\" >" +
                       $"<label for=\"about\"> {c.CertificationType} </label><br>";
                }


                ViewData["certs"] = certString;
            }


            return Page();


        }




        public IActionResult OnPost()
        {
            Account.PersistAll();
            ViewData["subTitle"] = $"<input type = \"text\" placeholder = \"Name this profile\" name=\"subName\" id=\"subName\" value = \"{subName}\" readonly> <br>";

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


            string edString = "";
            eduCollection = "";
            foreach (EducationData e in Account.SavedEducationHistory.Values)
            {
                eduCheck = "";
                foreach (string ed in eduHist)
                {
                    if (e.Institution == ed)
                    {
                        eduCheck = "checked";
                        break;
                    }
                }
                edString += $"<input type=\"checkbox\" name =\"eduHist\" value=\"{e.Institution}\" onChange = \"this.form.submit()\" {eduCheck}>" +
                $"<label for=\"about\"> {e.Institution} </label><br>";

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
                        break;
                    }
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
                        break;
                    }
                }

                skillString += $"<input type=\"checkbox\" name=\"skills\" value=\"{s.SkillName}\" onChange = \"this.form.submit()\" {skillCheck} >" +
               $"<label for=\"about\"> {s.SkillName} </label><br>";
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
                        break;
                    }


                    certString += $"<input type=\"checkbox\" name=\"certs\" value=\"{c.CertificationType}\" onChange = \"this.form.submit()\" {certCheck} >" +
                       $"<label for=\"about\"> {c.CertificationType} </label><br>";

                    if (certCheck == "checked")
                    {
                        certCollection += $"<br> {c.Institution}<br>{c.CertificationType} <br> {c.StartDate} to {c.EndDate}<br>";
                    }

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




            return Page();
        }
    }
}
