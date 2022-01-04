using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
public enum state{
    active,done,na
}

namespace Todoapi.Models
{
    /* Task model
    */
    public class task{

        public string Id {get; set;}
        
        [Required(ErrorMessage ="Title is required")]
        public string title {get; set;}

        [Required(ErrorMessage ="Details are required")]
        public string details {get; set;}

        [Required(ErrorMessage ="Due date is required")]
        public DateTime dueDate {get; set;}

        [JsonConverter(typeof(StringEnumConverter))]
        public state? status {get; set;
        }

        [Display(Name = "people")]
        public string ownerId {get; set;}

        [ForeignKey("ownerId")]  
        protected people peoples { get; set; }
        
    }
}