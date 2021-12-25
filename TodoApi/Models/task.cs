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

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] public string Id {get; set;}
        
        [Required(ErrorMessage ="Title is required")]
        public string title {get; set;}

        [Required(ErrorMessage ="Details are required")]
        public string details {get; set;}

        [Required(ErrorMessage ="Due date is required")]
        public DateTime dueDate {get; set;}

        [JsonConverter(typeof(StringEnumConverter))]
        public state? status {get; set;
        }

        public string ownerId {get; set;}
        
    }
}