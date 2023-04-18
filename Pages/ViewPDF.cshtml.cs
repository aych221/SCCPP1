using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc;
using SCCPP1.Database.Entity;
using SCCPP1.Database.Sqlite;
using SCCPP1.Session;
using SCCPP1.User;

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
        public List<CheckBoxViewModel> EducationHistory { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> Certifications { get; set; }


        [BindProperty]
        public List<CheckBoxViewModel> WorkHistory { get; set; }



        public IActionResult OnGet()
        {
            Console.WriteLine("[ViewPDFModel] OnGet called");
            Console.WriteLine($"Profiles loaded: {Account.SavedProfiles.Count}");
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

            // Do something with the selected skills

            return Page();
        }


    }
}