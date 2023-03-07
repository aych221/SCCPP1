﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
    public class IndexModel : SessionModel
	{

		private readonly ILogger<IndexModel> _logger;

		
		public IndexModel(SessionHandler sessionHandler, ILogger<IndexModel> logger) : base(sessionHandler)
        {
            _logger = logger;
		}


        [BindProperty]
        public string Username { get; set;}

        public IActionResult OnGet()
		{

            string page = "/Index";
            if (!(User == null) || !(User.Identity == null) || !User.Identity.IsAuthenticated)
            {
                page = sessionHandler.Login(this);
            }
            return RedirectToPage(page);

            //Username = HttpContext.Session.GetString("Username");

        }


        public IActionResult OnPost()
        {
            string btn = Request.Form["button"];
            string page = "/Index";

            
            switch (btn.ToLower())
            {
                case "login":
                    sessionHandler.Login(this);
                    page = "/Profile";
                    break;

                case "logout":
                    sessionHandler.Logout(this);
                    break;

                default:
                    break;
            }
            return RedirectToPage(page);
        }

    }
}