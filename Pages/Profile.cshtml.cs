using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Session;

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

		public static List<string> list;

	}
}