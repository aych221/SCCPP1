using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.Database.Tables;
using SCCPP1.User;
using System.Security.Claims;

namespace SCCPP1.Session
{
    public class SessionHandler
    {
        public static int HandlerCount;

        private SessionData Data { get; set; }


        public SessionHandler()
        {
            Console.WriteLine($"Created Handler (Count: {++HandlerCount})");
        }
        
        //Destructor, probably need to save account in desctructor
        ~SessionHandler()
        {
            Console.WriteLine($"Removed Handler (Count: {--HandlerCount})");
        }

        public Account GetAccount()
        {
            return Data.Owner;
        }

        //SessionData accessors
        public int GetID()
        {
            if (Data == null)
                return -1;

            return Data.ID;
        }

        public string? GetUsername()
        {
            if (Data == null)
                return null;

            return Data.Username;
        }

        public bool IsSignedOn()
        {
            if (Data == null)
                return false;


            return Data.IsAuthenticated();
        }

        //probably redundant since IsSignedOn can be used
        [Obsolete]
        public bool HasSession()
        {
            return Data != null;
        }

        
        public void SessionCheck(PageModel pm)
        {

            //pm.HttpContext.Session.Clear();

            if (!IsSignedOn())
                pm.RedirectToPage("/Index");
        }

        public string Login(PageModel pm)
        {
            string page;
            Data = new SessionData(pm.User);

            Console.WriteLine(Data.Username);


            //debug only
            Account acc;
            //DatabaseConnector.Thomas(acc = new Account(Data, false));
            //page = "/UserHome";
            //Console.WriteLine("[IsReturning = True]");
            //Debug use only
            //TODO: make easier to use debugs with SSO
            if (Data.Username.Equals("ww8FDk-bnuBk1KJXVreseNbsDmGnt62pNRpswwgGC7k"))
            {
                DatabaseConnector.Thomas(acc = new Account(Data, false));
                page = "/UserHome";
                Console.WriteLine("[IsReturning = True]");
                //new TableModels();
                //DatabaseConnector.TestQueryMaker();
                //Console.WriteLine(DatabaseConnector.LoadColleagueEducationHistory1(acc));
                //Console.WriteLine(DatabaseConnector.LoadColleagueEducationHistory(acc));
                //Console.WriteLine(DatabaseConnector.LoadColleagueWorkHistory1(acc));
                //Console.WriteLine(DatabaseConnector.LoadColleagueWorkHistory(acc));

            }
            else//*/ 
            if (DatabaseConnector.ExistsUser(Data.Username) < 1)
            {
                acc = new Account(Data, false);
                acc.Save();
                Console.WriteLine($"Account: {acc.EmailAddress}, {acc.Name}, {acc.GetUsername()}");

                //should be UpdateInfo page or something.
                page = "/CreateMainProfile";
                Console.WriteLine("[IsReturning = False]");
            }
            else
            {
                acc = DatabaseConnector.GetUser(Data.Username);
                page = "/UserHome";
                Console.WriteLine("[IsReturning = True]");
            }

            //save account to session.
            Data.Owner = acc;

            return page;
        }
        public string Login(string username)
        {
            /*
        @User.FindFirst("name")
        ;
        @User.FindFirst("preferred_username")
        ;
        @User.FindFirst("system.security.claims.claimtypes.nameidentifier");*/

            string page = "/Index";

            //null/empty string or already signed on
            if (string.IsNullOrEmpty(username) || (Data != null && IsSignedOn()))
                return page;

            Data = new SessionData(username);

            //debug only
            int id;
            Account acc;
            if (username.Equals("ww8FDk-bnuBk1KJXVreseNbsDmGnt62pNRpswwgGC7k"))
            {
                DatabaseConnector.Thomas(acc = new Account(Data, false));
                page = "/UserHome";
                
            }
            else if ((id = DatabaseConnector.ExistsUser(username)) < 1)
            {
                acc = new Account(Data, false);
                DatabaseConnector.SaveUser(acc);

                //should be UpdateInfo page or something.
                page = "/EditMainProfile";
            }
            else
            {
                acc = DatabaseConnector.GetUser(username);
                page = "/UserHome";
            }

            //save account to session.
            Data.Owner = acc;

            return page;
        }

        public void Logout(PageModel pm)
        {

            /*pm.Response.Cache.SetCacheability(pm.HttpCacheability.NoCache);
            pm.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            pm.Response.Cache.SetNoStore();*/

            /*pm.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            pm.Response.Headers["Expires"] = "0";
            pm.Response.Headers["Pragma"] = "no-cache";*/
            Data.SignedOn = false;
            pm.HttpContext.Session.Clear();

            pm.RedirectToPage("/Index");
        }

    }

}
