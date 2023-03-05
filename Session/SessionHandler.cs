using Microsoft.AspNetCore.Mvc.RazorPages;
using SCCPP1.User;
using System.Security.Claims;

namespace SCCPP1.Session
{
    public class SessionHandler
    {
        private SessionData _data { get; set; }


        public SessionHandler()
        {

        }

        public Account GetAccount()
        {
            return _data.Owner;
        }

        //SessionData accessors
        public int GetID()
        {
            if (_data == null)
                return -1;

            return _data.ID;
        }

        public string? GetUsername()
        {
            if (_data == null)
                return null;

            return _data.Username;
        }

        public bool IsSignedOn()
        {
            if (_data == null)
                return false;

            return _data.SignedOn;
        }

        //probably redundant since IsSignedOn can be used
        [Obsolete]
        public bool HasSession()
        {
            return _data != null;
        }

        
        public void SessionCheck(PageModel pm)
        {

            //pm.HttpContext.Session.Clear();

            if (!IsSignedOn())
                pm.RedirectToPage("/Index");
        }

        public string Login(PageModel pm)
        {
            ClaimsPrincipal cp = pm.User;
            string username = cp.FindFirstValue(ClaimTypes.NameIdentifier);
            string page;

            Console.WriteLine(username);
            if (username == null)
            {
                username = "brittl";
            }
            _data = new SessionData(username);


            //debug only
            int id;
            Account acc;
            if ((id = DatabaseConnector.ExistsUser(username)) < 1)
            {
                acc = new Account(_data, false);
                acc.Email = cp.FindFirstValue("preferred_username");
                acc.Name = cp.FindFirstValue("name");
                DatabaseConnector.SaveUser(acc);
                Console.WriteLine($"Account: {acc.Email}, {acc.Name}, {acc.GetUsername()}");

                //should be UpdateInfo page or something.
                page = "/CreateMainProfile";
                Console.WriteLine("[IsReturning = False]");
            }
            else
            {
                acc = DatabaseConnector.GetUser(username);
                page = "/UserHome";
                Console.WriteLine("[IsReturning = True]");
            }

            //save account to session.
            _data.Owner = acc;

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
            if (string.IsNullOrEmpty(username) || (_data != null && IsSignedOn()))
                return page;

            _data = new SessionData(username);

            //debug only
            int id;
            Account acc;
            if ((id = DatabaseConnector.ExistsUser(username)) < 1)
            {
                acc = new Account(_data, false);
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
            _data.Owner = acc;

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
            _data.SignedOn = false;
            pm.HttpContext.Session.Clear();

            pm.RedirectToPage("/Index");
        }

    }

}
