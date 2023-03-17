using SCCPP1.User;
using System.Security.Claims;

namespace SCCPP1.Session
{
    public class SessionData
    {
        //This is the session ID, not necessarily the Colleages ID
        public int ID { get; set; }
        public string Username { get; set; }

        [Obsolete("Use IsAuthenticated instead.")]
        public bool SignedOn { get; set; }

        public Account Owner  { get; set; }

        //user that's logged in
        public ClaimsPrincipal User { get; set; }


        public SessionData(string username)
        { 
            this.SignedOn = true;
            this.Username = username;
        }

        public SessionData(ClaimsPrincipal sessionUser)
        {
            this.User = sessionUser;


            this.SignedOn = IsAuthenticated();
            this.Username = GetUsersNameIdentifier();
        }



        public bool IsAuthenticated()
        {
            if (User == null || User.Identity == null)
                return false;

            return User.Identity.IsAuthenticated;

        }



        /// <summary>
        /// Gets the authentication user's email, based on what is used as their Microsoft account.
        /// </summary>
        /// <returns>User's email</returns>
        public string GetUsersEmail()
        {
            if (User == null)
                return Owner.EmailAddress;
            //return User.FindFirstValue(ClaimTypes.Email);
            return User.FindFirstValue("preferred_username");
        }

        /// <summary>
        /// Gets the authentication user's full name, based on what is displayed on their Microsoft account.
        /// </summary>
        /// <returns>User's full name</returns>
        public string GetUsersName()
        {
            if (User == null)
                return Owner.Name;
            //return User.FindFirstValue(ClaimTypes.Name);
            return User.FindFirstValue("name");
        }

        /// <summary>
        /// Gets the authentication user's name identifier for their Microsoft account.
        /// </summary>
        /// <returns>User's name identifier</returns>
        public string GetUsersNameIdentifier()
        {

            if (User == null)
                return Owner.GetUsername();
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

    }
}
