using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
	public class PrivacyModel : PageModel
    {
        public readonly SessionHandler sessionHandler;
        private readonly ILogger<PrivacyModel> _logger;

		public PrivacyModel(SessionHandler sessionHandler, ILogger<PrivacyModel> logger)
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