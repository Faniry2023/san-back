namespace SAN_API.Models
{
    public class LoginModel
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email {  get; set; }
        public string? Password { get; set; }
    }
}
