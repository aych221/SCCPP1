using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
	public class UserHomeModel : SessionModel
    {
        private readonly ILogger<UserHomeModel> _logger;

        public UserHomeModel(SessionHandler sessionHandler, ILogger<UserHomeModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}