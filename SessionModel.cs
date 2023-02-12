using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SCCPP1
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
