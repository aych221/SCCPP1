using Microsoft.AspNetCore.Mvc;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
    public class CreateSubProfileModel : SessionModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public CreateSubProfileModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger) : base(sessionHandler) 
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}