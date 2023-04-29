using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SCCPP1.Database;
using SCCPP1.Database.Requests;
using SCCPP1.Database.Tables;
using SCCPP1.User;
using System.Security.Claims;
using static SCCPP1.Database.Requests.DbRequest;

namespace SCCPP1.Session
{
    public class SessionHandler
    {
        public static int HandlerCount;

        private SessionData Data { get; set; }


        public SessionHandler()
        {
#if DEBUG
            Console.WriteLine($"Created Handler (Count: {++HandlerCount})");
#endif
        }
        
        //Destructor, probably need to save account in desctructor
        ~SessionHandler()
        {
#if DEBUG
            Console.WriteLine($"Removed Handler (Count: {--HandlerCount})");
#endif
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

#if DEBUG
            Console.WriteLine(Data.Username);
#endif


            //debug only
            Account acc;
            //DatabaseConnector.Thomas(acc = new Account(Data, false));
            //page = "/UserHome";
            //Console.WriteLine("[IsReturning = True]");
            //Debug use only
            //TODO: make easier to use debugs with SSO

            if (Program.DbRequestSystem)
            {
#if DEBUG
                if (Data.Username.Equals("ww8FDk-bnuBk1KJXVreseNbsDmGnt62pNRpswwgGC7k"))
                {

 //                   DatabaseConnector.Andrew(acc = new Account(Data, false));
                    acc = RetrieveAccount();
                    if (DbConstants.STARTUP_ADD_MOCK_DATA && DbConstants.STARTUP_RESET_TABLES)
                        DatabaseConnector.Andrew(acc = new Account(Data, false));

                    if (acc == null)
                    {
#if DEBUG
                        Console.WriteLine($"Account was null");
#endif
                        acc = new Account(Data, false);
                        page = "/CreateMainProfile";
                    }
                    else
                    {
#if DEBUG
                        Console.WriteLine($"Account was loaded.");
#endif
                        page = "/UserHome";
                    }
                }
                else
                {
#endif
                    //shouldn't ever be null unless database is down.
                    acc = RetrieveAccount();
                    if (acc == null)
                    {
                        acc = new Account(Data, false);
                        page = "/CreateMainProfile";
                    }
                    else
                    {
                        page = "/UserHome";
                    }
#if DEBUG
                }
#endif
            }
            else
            {
#if DEBUG
                if (Data.Username.Equals("ww8FDk-bnuBk1KJXVreseNbsDmGnt62pNRpswwgGC7k"))
                {
                    DatabaseConnector.Andrew(acc = new Account(Data, false));
                    page = "/UserHome";
                    //new TableModels();
                    //DatabaseConnector.TestQueryMaker();
                    //Console.WriteLine(DatabaseConnector.LoadColleagueEducationHistory1(acc));
                    //Console.WriteLine(DatabaseConnector.LoadColleagueEducationHistory(acc));
                    //Console.WriteLine(DatabaseConnector.LoadColleagueWorkHistory1(acc));
                    //Console.WriteLine(DatabaseConnector.LoadColleagueWorkHistory(acc));

                }
                else
#endif
                if ((acc = DatabaseConnector.GetAccount(Data)).IsReturning)
                {
                    page = "/UserHome";
                }
                else
                {
                    page = "/CreateMainProfile";
                }
            }
#if DEBUG
            Console.WriteLine($"[IsReturning = {acc.IsReturning}]");
#endif

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

        public virtual Account RetrieveAccount()
        {

            DbRequest request = DbRequestManager.LoadAccount(Data);
#if DEBUG
            Console.WriteLine("Awaiting result");
#endif
            while (request.Status != RequestStatus.COMPLETED) ;
#if DEBUG
            Console.WriteLine("returning result.");
#endif

            if (request.ResultAsObject() == null)
                return new Account(Data, false);

            Account account = request.ResultAsObject() as Account;
            account.PersistAll();
            return account;

        }

    }

}
