using Microsoft.AspNetCore.Mvc;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
    public class EditSubProfileModel : SessionModel
    {
        private readonly ILogger<EditSubProfileModel> _logger;

        public EditSubProfileModel(SessionHandler sessionHandler, ILogger<EditSubProfileModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return Page();
        }
    }
}