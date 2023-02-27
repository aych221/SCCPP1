﻿using System.Security.Cryptography;
using System.Text;

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

        public static DateOnly ToDateOnly(string input)
        {
            DateOnly date = new DateOnly();
            bool pass = DateOnly.TryParse(input, out date); //need to set a fail condition
            return date;
        }

        //TODO
        public static DateOnly ToDateOnly(DateTime input)
        {
            //DateOnly date = DateOnly.FromDateTime(input);
            return DateOnly.FromDateTime(input);
        }

        //TODO
        public static DateTime FromDateOnly(DateOnly input)
        {
            return input.ToDateTime(new TimeOnly(0, 0));
        }

    }
}
