namespace WostupServer
{
    public sealed record CreateFilesRequest
    {
        public required List<IFormFile> Files { get; set; }
        public required string UserName { get; set; }
        public required string UserNumber { get; set; }
        public required string ServerDestinationPath { get; set; }
    }
}
