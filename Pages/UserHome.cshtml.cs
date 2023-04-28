
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Session;
using SCCPP1.User.Data;

namespace SCCPP1.Pages
{
    public class UserHomeModel : SessionModel
    {
        private readonly ILogger<UserHomeModel> _logger;

        public UserHomeModel(SessionHandler sessionHandler, ILogger<UserHomeModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }

        [BindProperty]
        public string subP { get; set; }

        [BindProperty]
        public string? newProf { get; set; }

        [BindProperty]
        public string newSubP { get; set; }


        public IActionResult OnGet()
        {
            


            string st = "";

            foreach (ProfileData e in Account.SavedProfiles.Values)
            {
                st += $"<div id =\"{e.Title}\" class = \"subp\"> <i style='font-size:120px' class='far'>&#xf15c;</i> <input type = \"submit\" class = \"subP\" name = \"subP\" Value = \"{e.Title}\" > </div>";
            }
            ViewData["subProfiles"] = st;



            

            return Page();
        }

        public IActionResult OnPost()
        {
            
            if (newProf=="Submit")
            {
                Account.ChooseProfile(Account.CreateProfile(newSubP));
            }
            else
            {
                foreach (ProfileData e in Account.SavedProfiles.Values)
                {
                    if (e.Title == subP)
                    {
                        Account.ChooseProfile(e);
                        break;
                    }
                }
            }
            newProf = "";
            newSubP = "";
            
            
            return RedirectToPage("viewPDF");
        }



    }
}