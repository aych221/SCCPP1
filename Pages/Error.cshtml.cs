using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using SCCPP1.Session;

namespace SCCPP1.Pages
{
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	[IgnoreAntiforgeryToken]
	public class ErrorModel : PageModel
	{
		public string? RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

		public readonly SessionHandler sessionHandler;
		private readonly ILogger<ErrorModel> _logger;

		public ErrorModel(SessionHandler sessionHandler, ILogger<ErrorModel> logger)
		{
			this.sessionHandler = sessionHandler;
			_logger = logger;
		}

		public IActionResult OnGet()
		{
			RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

			return Page();
		}
	}
}