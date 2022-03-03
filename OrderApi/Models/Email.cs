
namespace OrderApi.Models
{
    public class Email
    {
        public int Id { get; set; }        
        public string Destination { get; set; }
        public string Content { get; set; }
    }
}
