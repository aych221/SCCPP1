using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SCCP
{
    public class SessionHandler
    {
        private SessionData _data { get; set; }


        public SessionHandler()
        {

        }


        //Session Data accessors
        public int GetID()
        {
            if (_data == null)
                return -1;

            return _data.ID;
        }

        public string GetUsername()
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

        public void Login(string username)
        {

            //null/empty string or already signed on
            if (string.IsNullOrEmpty(username) || (_data != null && IsSignedOn()))
                return;


            _data = new SessionData(username);

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
