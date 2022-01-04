using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace Todoapi.Models
{
    public class people{
        [Required(ErrorMessage ="Name is required")]
        public string name {get; set;}
        [Required(ErrorMessage ="Email is required")]
        public string email {get; set;}
        [Required(ErrorMessage ="Favorite Programming Language is required")]
        public string favoriteProgrammingLanguage {get; set;}

        public int? activeTaskCount {get; set;}
        
        public string Id {get; set;}

        

        
    }
}