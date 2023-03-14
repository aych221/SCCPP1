using System.ComponentModel.DataAnnotations;

namespace SCCPP1.Models
{
    // Here is the Profile class and it serves to combine info that should linked together
    // Considerations and changes are still to be made on this.
    // ie. connect First Name, Middle Name, Last Name
    public class Profile
    {
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string? Name { get; set; }
    }
}