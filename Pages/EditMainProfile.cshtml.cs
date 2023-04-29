using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User;
using System.Collections.ObjectModel;
using System.Linq;

namespace SCCPP1.Pages
{
    public class EditMainProfileModel : SessionModel
    {
        private readonly ILogger<EditMainProfileModel> _logger;

        public EditMainProfileModel(SessionHandler sessionHandler, ILogger<EditMainProfileModel> logger) : base(sessionHandler)
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

        [BindProperty]
        public List<SkillDataModel> SavedSkills { get; set; }

        [BindProperty]
        public List<WorkDataModel> SavedWork { get; set; }

        [BindProperty]
        public List<EducationDataModel> SavedEducation { get; set; }

        [BindProperty]
        public List<CertificationDataModel> SavedCertifications { get; set; }


        
        public abstract class RecordDataModel
        {
            public int RecordID { get; set; }
        }

        public class SkillDataModel : RecordDataModel
        {
            public string Category { get; set; }
        }

        public class EducationDataModel : RecordDataModel { }

        public class CertificationDataModel : RecordDataModel { }

        public class WorkDataModel : RecordDataModel { }


        protected void PopulateRecordLists()
        {
            SavedSkills = Account.SavedSkills
                .Select(v => new SkillDataModel
                {
                    RecordID = v.Value.RecordID,
                    Category = v.Value.SkillCategoryName

                }).ToList();

            SavedEducation = Account.SavedEducationHistory
                .Select(v => new EducationDataModel
            {
                RecordID = v.Value.RecordID
            }).ToList();

            SavedCertifications = Account.SavedCertifications
                .Select(v => new CertificationDataModel
            {
                RecordID = v.Value.RecordID
            }).ToList();

            SavedWork = Account.SavedWorkHistory
                .Select(v => new WorkDataModel
            {
                RecordID = v.Value.RecordID
            }).ToList();
        }
        


        // When this page is initialize OnGet() starts before the instance of the user is created, therefore within our OnGet(),
        // We create a instance of the user and pull the saved data from the database to be displayed.
        public IActionResult OnGet()
        {
            Console.WriteLine("EditMainProfile.OnGet() Called");
            if (Colleague == null)
                Colleague = new Colleague();

            Console.WriteLine("Model State is valid and Account is returning");
            // TODO: add name fields in DB
            string[] names = Utilities.SplitFullName(Account.Name);
            Colleague.FirstName = names[1];
            Colleague.LastName= names[0];
            Colleague.MiddleName = names[2];
            Colleague.PhoneNumber = Account.PhoneNumber;
            Colleague.IntroNarrative = Account.IntroNarrative;
            Colleague.EmailAddress = Account.EmailAddress;

            PopulateRecordLists();

            return Page();
        }

        private delegate void Remove(int id);

        // When the Submit button on the form is pressed on, OnPost() starts, grabs the information the user typed and then saves it into the database.
        // If it is successfully saved into the database, redirect the user to "/UserHome", if it fails, return this page.
        public IActionResult OnPost(
            List<SkillDataModel> SavedSkills,
            List<EducationDataModel> SavedEducation,
            List<CertificationDataModel> SavedCertifications,
            List<WorkDataModel> SavedWork)
        {
            Console.WriteLine("EditMainProfile.OnPost() Called");

            //first remove all records not on these lists
            this.SavedSkills = SavedSkills;
            this.SavedEducation = SavedEducation;
            this.SavedCertifications = SavedCertifications;
            this.SavedWork = SavedWork;

            Console.WriteLine("Skills found: " + SavedSkills.Count);
            Console.WriteLine("Edus found: " + SavedEducation.Count);
            Console.WriteLine("Certs found: " + SavedCertifications.Count);
            Console.WriteLine("Work found: " + SavedWork.Count);

            //ids to keep (makes this process 2n instead of n^2)
            HashSet<int> keepIDs = new HashSet<int>();
            int removed = 0;

            void ValidateRecordData<T, K>(List<K> list, ReadOnlyDictionary<int, T> dict, Remove r)
                where T : RecordData
                where K : RecordDataModel
            {
                int removed = 0;
                HashSet<int> keepIDs = new HashSet<int>();

                foreach (var v in list)
                    keepIDs.Add(v.RecordID);

                foreach (var v in dict)
                {
                    if (!keepIDs.Contains(v.Value.RecordID))
                    {
                        r(v.Value.RecordID);
                        removed++;
                    }
                }
                Console.WriteLine("Removed " + removed + " records");
            }


            ValidateRecordData(SavedSkills, Account.SavedSkills, Account.RemoveSkill);
            ValidateRecordData(SavedEducation, Account.SavedEducationHistory, Account.RemoveEducation);
            ValidateRecordData(SavedCertifications, Account.SavedCertifications, Account.RemoveCertification);
            ValidateRecordData(SavedWork, Account.SavedWorkHistory, Account.RemoveWork);
            /*
            //remove skills
            foreach (var skill in SavedSkills)
                keepIDs.Add(skill.SkillID);

            foreach (var skill in Account.SavedSkills)
            {
               if (!keepIDs.Contains(skill.Value.RecordID))
                    Account.RemoveSkills(skill.Value.RecordID);
            }
            keepIDs.Clear();*/


            //remove education
/*            foreach (var edu in SavedEducation)
                keepIDs.Add(edu.EduID);

            foreach (var edu in Account.SavedEducationHistory)
            {
                if (!keepIDs.Contains(edu.Value.RecordID))
                {
                    Account.RemoveEducations(edu.Value.RecordID);
                    removed++;
                }
            }
            Console.WriteLine("Removed " + removed + " Education records");
            keepIDs.Clear();
            removed = 0;



            foreach (var certs in SavedCertifications)
                keepIDs.Add(certs.CertID);

            foreach (var certs in Account.SavedCertifications)
            {
                if (!keepIDs.Contains(certs.Value.RecordID))
                {
                    Account.RemoveCertifications(certs.Value.RecordID);
                    removed++;
                }
            }
            Console.WriteLine("Removed " + removed + " Certification records");
            keepIDs.Clear();
            removed = 0;




            foreach (var work in SavedWork)
                keepIDs.Add(work.WorkID);

            
            foreach (var work in Account.SavedWorkHistory)
            {
                if (!keepIDs.Contains(work.Value.RecordID))
                {
                    Account.RemoveWorks(work.Value.RecordID);
                    removed++;
                }
            }
            Console.WriteLine("Removed " + removed + " Work records");
            keepIDs.Clear();
            removed = 0;*/



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


                

                // Save the "Skills" of the user
                // Programming Languages
                if (Skill.ProgLang != null)
                {
                    var plang = Request.Form["Skill.ProgLang"].ToString().Split(',');
                    Account.AddSkills("Programming Languages", plang);
                }

                // Operating Systems
                if (Skill.OS != null)
                {
                    var osys = Request.Form["Skill.OS"].ToString().Split(',');
                    Account.AddSkills("OS", osys);
                }

                // Software & Framework
                if (Skill.SoftAndFrame != null)
                {
                    var snf = Request.Form["Skill.SoftAndFrame"].ToString().Split(',');
                    Account.AddSkills("Software and Framework", snf);
                }

                // Other Skills
                if (Skill.Other != null)
                {
                    var other = Request.Form["Skill.Other"].ToString().Split(',');
                    Account.AddSkills("Other", other);
                }



                // Save the "Experience" of the user
                string hiddenExp = Request.Form["Work.Experience"];
                if (hiddenExp != null)
                {
                    var experience = Request.Form["Work.Experience"].ToString().Split(';');
                    for (int i = 0; i < experience.Length - 1; i++)
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


                // Save the "Education" of the user
                string hiddenEdu = Request.Form["Education.Value"];
                if (hiddenEdu != null)
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


                // Save the "Certification" of the user
                string hiddenCert = Request.Form["Certification.Value"];
                if (hiddenCert != null)
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

                Account.PersistAll();
            }

            return RedirectToPage("UserHome");
        }
    }
}