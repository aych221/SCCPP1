using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SCCP
{
    public class SessionModel : PageModel
    {
        public readonly SessionHandler sessionHandler;

        public SessionModel(SessionHandler sessionHandler)
        {
            this.sessionHandler = sessionHandler;
        }
    }
}
