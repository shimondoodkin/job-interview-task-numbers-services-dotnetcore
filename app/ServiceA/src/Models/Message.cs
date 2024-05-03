namespace ServiceA.Models
{
    public class Message
    {
        public int Id { get; set; }
        public required string Content { get; set; }
        public int RandomNumber { get; set; }
        public bool Processed { get; set; } = false;  // New boolean field, defaulting to false

    }
}
