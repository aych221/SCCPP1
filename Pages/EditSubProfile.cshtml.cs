using Microsoft.AspNetCore.Mvc;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
    public class EditSubProfileModel : SessionModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public EditSubProfileModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}