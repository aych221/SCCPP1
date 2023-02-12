using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCP;
using SCCP.Pages;

namespace SCCPP1.Pages
{
	public class ViewPDFModel : PageModel
    {
        public readonly SessionHandler sessionHandler;
        private readonly ILogger<PrivacyModel> _logger;

        public ViewPDFModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger)
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