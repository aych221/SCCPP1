using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.Pages;
using SCCPP1.User;

namespace SCCPP1.Session
{
    public class SessionModel : PageModel
    {

        public SessionData SessionData;

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
            if (this is not IndexModel)
                SessionData = Account.Data;
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
