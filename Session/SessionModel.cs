using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.User;

namespace SCCPP1.Session
{
    public class SessionModel : PageModel
    {

        public Account Account { 
            get
            {
                return sessionHandler.GetAccount();
            }
        }

        public readonly SessionHandler sessionHandler;

        public SessionModel(SessionHandler sessionHandler)
        {
            this.sessionHandler = sessionHandler;
        }

        ~SessionModel()
        {
            Console.WriteLine("[SessionModel] destructor called");
            PersistAll();
        }


        public bool PersistAll()
        {
            return sessionHandler.GetAccount().PersistAll();
        }
    }
}
