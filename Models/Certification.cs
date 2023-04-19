using System.ComponentModel.DataAnnotations;
using SCCPP1.User.Data;

namespace SCCPP1.Models
{
    public class Certification
    {
        public int ID { get; set; }

        public string Value { get; set; }

        public string? Institution { get; set; }

        public string? Certificate { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}