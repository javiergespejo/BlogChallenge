using BlogChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogChallenge.DTO
{
    public class PostDTO
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
