using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RedditWebRole.Models
{
    public class EditViewModel
    {
        [Required]
        [Display(Name = "FirstName")]
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
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Slično kao kod RegisterViewModel, neće biti Required jer nije obavezno
        [Display(Name = "Sličica")]
        public string ImageUrl { get; set; }
    }
}