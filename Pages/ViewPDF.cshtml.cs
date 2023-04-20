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

        [BindProperty]
        public List<CheckBoxViewModel> Skills { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> Buttons { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> EducationHistory { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> Certifications { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> WorkHistory { get; set; }

        private ProfileData p;

        public IActionResult OnGet()
        {

            p = Account.ChosenProfile();

            List<CheckBoxViewModel> LoadModel<T>(ReadOnlyDictionary<int, T> dict) where T : RecordData
            {
                return dict.Select(v => new CheckBoxViewModel
                {
                    RecordID = v.Value.RecordID,
                    IsSelected = true
                }).ToList();
            }

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
            foreach (var v in Skills)
            {

                if (v.IsSelected)
                {
                    if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Programming Languages")
                    {
                        progLangs += $"<div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "OS") 
                    {
                        opSys += $"<div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Software and Framework")
                    {
                        sF += $"<div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";

                    }
                    else
                    {
                        other += $"<div style = \"display:block\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";

                    }
                }
                else
                {
                    if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Programming Languages")
                    {
                        progLangs += $"<div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "OS")
                    {
                        opSys += $"<div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";
                    }
                    else if (Account.GetSkillData(v.RecordID).SkillCategoryName == "Software and Framework")
                    {
                        sF += $"<div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";

                    }
                    else
                    {
                        other += $"<div style = \"display:none\" id = \"skills_{v.RecordID}\"> {Account.GetSkillData(v.RecordID).SkillName} </div> <br> ";

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
                if (v.IsSelected)
                {
                    Console.WriteLine($"EDU {v.RecordID} is selected");
                    edu += $"<div style = \"display:block\" id =\"edu_{v.RecordID}\">" + Account.GetEducationData(v.RecordID).Institution + "<br>" + Account.GetEducationData(v.RecordID).EducationType + "<br>" + Account.GetEducationData(v.RecordID).Description + "<br>" + "From: " + Account.GetEducationData(v.RecordID).StartDate + "to " + Account.GetEducationData(v.RecordID).EndDate + "</div><br>";
                }
                else
                {
                    edu += $"<div style = \"display:none\" id =\"edu_{v.RecordID}\">" + Account.GetEducationData(v.RecordID).Institution + "<br>" + Account.GetEducationData(v.RecordID).EducationType + "<br>" + Account.GetEducationData(v.RecordID).Description + "<br>" + "From: " + Account.GetEducationData(v.RecordID).StartDate + "to " + Account.GetEducationData(v.RecordID).EndDate + "</div><br>";

                }
            }
            //if (edu != "")
            //{
            //    edu = $"<div class=\"education\"><h5><i class=\"fa fa-graduation-cap\"></i><b> Education </b></h5><p id=\"blue\">{edu}</p></div>";
            //}
            ViewData["eduDisplay"] = edu;


            string certs = "";
            foreach (var v in Certifications)
            {
                if (v.IsSelected)
                {
                    Console.WriteLine($"Skill {v.RecordID} is selected");
                    certs += $"<div style = \"display:block\" id = \"certs_{v.RecordID}\"> {Account.GetCertificationData(v.RecordID).CertificationType} </div> <br> ";
                }
                else
                {
                    certs += $"<div style = \"display:none\" id = \"certs_{v.RecordID}\"> {Account.GetCertificationData(v.RecordID).CertificationType} </div> <br> ";
                }
            }
            //if (certs != "")
            //{
            //    certs = $"<div class=\"education\"><h5><i class=\"fa fa-graduation-cap\"></i><b> Certiications </b></h5><p id=\"blue\">{certs}</p></div>";
            //}
            ViewData["certDisplay"] = certs;



            string work = "";
            foreach (var v in WorkHistory)
            {
                if (v.IsSelected)
                {
                    Console.WriteLine($"Skill {v.RecordID} is selected");
                    work += $"<div style = \"display:block\" id = \"work_{v.RecordID}\"> <b>{Account.GetWorkData(v.RecordID).Employer}</b> <br> {Account.GetWorkData(v.RecordID).JobTitle} <br> {Account.GetWorkData(v.RecordID).Description} <br> {Account.GetWorkData(v.RecordID).StartDate} to {Account.GetWorkData(v.RecordID).EndDate} <br> {Account.GetWorkData(v.RecordID).Description}  </div> <br> ";
                }
                else
                {
                    work += $"<div style = \"display:none\" id = \"work_{v.RecordID}\"> <b>{Account.GetWorkData(v.RecordID).Employer}</b> <br> {Account.GetWorkData(v.RecordID).JobTitle} <br> {Account.GetWorkData(v.RecordID).Description} <br> {Account.GetWorkData(v.RecordID).StartDate} to {Account.GetWorkData(v.RecordID).EndDate} <br> {Account.GetWorkData(v.RecordID).Description} </div> <br> ";
                }
            } 
            ViewData["workDisplay"] = work;


            return Page();
        }

        public IActionResult OnPost(List<CheckBoxViewModel> Skills, List<CheckBoxViewModel> EducationHistory, List<CheckBoxViewModel> Certifications, List<CheckBoxViewModel> WorkHistory)
        {
            // Handle form submission
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ERROR");
                return Page();
            }

            p = Account.ChosenProfile();

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

            Account.PersistAll();
            return RedirectToPage("UserHome");

        }
    }
}

