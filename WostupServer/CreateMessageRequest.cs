namespace WostupServer
{
    public sealed record CreateMessageRequest
    {
        public required string RecipientNumber { get; set; }
        public required string SenderNumber { get; set; }
        public required string Content { get; set; }
        public required DateTime SentDateTime { get; set; }
    }
}
