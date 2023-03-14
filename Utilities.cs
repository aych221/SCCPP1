using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SCCPP1
{
    public static class Utilities
    {

        public static string ToSHA256Hash(string input)
        {
            StringBuilder s = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                foreach (byte b in result)
                    s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }
        
        //changes format of date
        public static DateOnly ToDateOnly(string input)
        {
            DateOnly date = new DateOnly();
            bool pass = DateOnly.TryParse(input, out date); //Fail condition rendered unnecessary by using date HTML input
            return date;
        }

        //Using datetime to transform to date
        public static DateOnly ToDateOnly(DateTime input)
        {
            //DateOnly date = DateOnly.FromDateTime(input);
            return DateOnly.FromDateTime(input);
        }

        //reverse to DateTime
        public static DateTime FromDateOnly(DateOnly input)
        {
            return input.ToDateTime(new TimeOnly(0, 0));
        }


        public static string[] SplitFullName(string fullName)
        {
            string[] names = new string[3];

            //first, isolate the lastName
            string[] splits = fullName.Split(',');

            //Last name (Wall, Thomas Joseph) -> get Wall
            names[0] = splits[0];

            //First name (Thomas Joseph) -> get Thomas
            splits = splits[1].Trim().Split(' ');

            names[1] = splits[0];


            //Middle name(s) (Thomas Joseph) -> get Joseph
            if (splits.Length == 1)
            {
                names[2] = "";
            }
            else
            {
                StringBuilder sb = new StringBuilder(splits[1]);
                for (int i = 2; i < splits.Length; i++)
                    sb.Append($" {splits[i]}");

                names[2] = sb.ToString();
            }

            return names;
        }

        //to be rendered unnecessary by using HTML Phone input
        public static long ParsePhoneNumber(string phoneNumber)
        {
            //int num = -1;

            //phoneNumber = Regex.Replace(phoneNumber, "[0-9]+", "");
            //phoneNumber = phoneNumber.Trim().Replace("(", "");
            //return long.Parse(Regex.Replace(phoneNumber, "[0-9]+", ""));
            return long.Parse(phoneNumber.Trim().Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", ""));
        }


        // Prevent injection & fascilitate auto-input of data by preventing HTML code from being inserted
        public static string HtmlStripper(string input)
        {
            string strippedString = "";
            bool bracketFlag = false;
            char[] chars = input.ToCharArray();


            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '>' && bracketFlag)
                    bracketFlag = false;
                else if (chars[i] != '<' && !bracketFlag)
                    strippedString = strippedString + chars[i];
                else if (bracketFlag)
                    continue;
                else
                    bracketFlag = true;


            }

            return strippedString;
        }


        //Prevents any CS from running by stripping anything with @
        public static string CodeStripper(string input)
        {
            string strippedstring = "";
            char[] chars = input.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '@')
                    break;
                strippedstring += chars[i];
            }

            return strippedstring;
        }

    }
}
