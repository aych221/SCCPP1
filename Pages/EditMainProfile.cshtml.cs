using Microsoft.AspNetCore.Mvc;
using SCCPP1.Models;
using SCCPP1.Session;
using SCCPP1.User;

namespace SCCPP1.Pages
{
    public class EditMainProfileModel : SessionModel
    {
        private readonly ILogger<EditMainProfileModel> _logger;

        public EditMainProfileModel(SessionHandler sessionHandler, ILogger<EditMainProfileModel> logger) : base(sessionHandler)
        {
            _logger = logger;
        }

        [BindProperty]
        public Colleague? Colleague { get; set; }

        [BindProperty]
        public Work? Work { get; set; }

        [BindProperty]
        public Skill? Skill { get; set; }

        [BindProperty]
        public Education? Education { get; set; }

        public IActionResult OnGet()
        {
            Console.WriteLine("EditMainProfile.OnGet() Called");
            //invalid model state or the account is new
            if (!ModelState.IsValid || !Account.IsReturning)
                return Page();

            if (Colleague == null)
                Colleague = new Colleague();

            Console.WriteLine("Model State is valid and Account is returning");
            //TODO: add name fields in DB
            string[] names = Utilities.SplitFullName(Account.Name);
            Colleague.FirstName = names[1];
            Colleague.LastName= names[0];
            Colleague.MiddleName = names[2];
            Colleague.PhoneNumber = Account.Phone.ToString();
            Colleague.IntroNarrative = Account.IntroNarrative;
            Colleague.EmailAddress = Account.Email;

            return Page();
        }

        //TODO: should probably make EditMain and CreateMain same page and just change name.
        public IActionResult OnPost()
        {
            Console.WriteLine("EditMainProfile.OnPost() Called");
            if (!ModelState.IsValid)
                return Page();

            if (Colleague != null)
            {
                Account.Name = $"{Colleague.LastName}, {Colleague.FirstName} {Colleague.MiddleName?.ToString()}";
                Account.Email = Colleague.EmailAddress;
                Account.Phone = Utilities.ParsePhoneNumber(Colleague.PhoneNumber);
                Account.IntroNarrative = Colleague.IntroNarrative;

                if (DatabaseConnector.SaveUser(Account))
                {
                    Console.WriteLine("Saved");
                    ViewData["UserData"] = "Saved!";
                }
                else
                {
                    Console.WriteLine("Could not save");
                    ViewData["UserData"] = "Error Saving";
                }

            }

            return Page();
        }
    }
}