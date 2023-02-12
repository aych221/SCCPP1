﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1;
using SCCPP1.Pages;

namespace SCCPP1.Pages
{
	public class EditMainProfileModel : PageModel
    {
        public readonly SessionHandler sessionHandler;
        private readonly ILogger<PrivacyModel> _logger;

        public EditMainProfileModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger)
        {
            this.sessionHandler = sessionHandler;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}