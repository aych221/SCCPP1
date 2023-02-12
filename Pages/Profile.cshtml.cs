using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SCCPP1.Pages
{
	public class ProfileModel : SessionModel
	{
        private readonly ILogger<ProfileModel> _logger;

		public ProfileModel(SessionHandler sessionHandler, ILogger<ProfileModel> logger) : base(sessionHandler)
		{
			_logger = logger;
		}

		public IActionResult OnGet()
		{
			if (!sessionHandler.IsSignedOn())
				return RedirectToPage("/Index");

			//verify and load session
			return Page();
		}
	}
}