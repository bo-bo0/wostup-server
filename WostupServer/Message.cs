namespace WostupServer
{
    public sealed record Message
    {
        public int Id { get; set; }
        public string? RecipientNumber { get; set; }
        public string? SenderNumber { get; set; }
        public string? Content { get; set; }
        public DateTime SentDateTime { get; set; }
    }
}
