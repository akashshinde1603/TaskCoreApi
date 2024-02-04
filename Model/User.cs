using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskCoreApi.Model
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString(); // You can generate a unique identifier on the server side

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
  
        public bool IsAdmin { get; set; } = false;

        [Required]
        public int Age { get; set; }

        [Required]
        public string Hobbies { get; set; } = "[]";

        // Deserialize hobbies from JSON when needed
        //public List<string> HobbiesList => Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(Hobbies);
    }

    [NotMapped]
    public class UserLogin
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
