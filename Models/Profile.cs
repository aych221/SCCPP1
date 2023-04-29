using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    public class Profile
    {
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string? Name { get; set; }
    }
}