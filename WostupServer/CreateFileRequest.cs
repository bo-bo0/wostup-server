namespace WostupServer
{
    public sealed record CreateFileRequest
    {
        public required IFormFile File { get; set; }
        public required string UserName { get; set; }
        public required string UserNumber { get; set; }
    }
}
