using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BlogChallenge.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [StringLength(15)]
        [Display(Name = "Category")]
        public string Name { get; set; }
    }
}
