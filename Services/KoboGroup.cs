namespace SAN_API.Services
{
    public class KoboGroup
    {
        public string GroupName { get; set; }
        public List<string> Questions { get; set; } = new();
    }
}
