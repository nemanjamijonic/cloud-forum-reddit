using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RedditWebRole.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Korisničko ime (Email)")]
        [EmailAddress]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Ime")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Prezime")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Adresa")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Grad")]
        public string City { get; set; }

        [Required]
        [Display(Name = "Država")]
        public string Country { get; set; }

        [Required]
        [Display(Name = "Broj telefona")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Lozinka")]
        public string Password { get; set; }

        [Display(Name = "Sličica")]
        public HttpPostedFileBase ImageFile { get; set; }
    }
}
